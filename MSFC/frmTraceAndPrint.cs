using Microsoft.EntityFrameworkCore;
using MSFC.Data;
using MSFC.Dto;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSFC
{
    public partial class frmTraceAndPrint : Form
    {
        private readonly IDbContextFactory<MMesDbContext> _dbFactory;
        private PrintDocument printDoc = new PrintDocument();
        string _SettingEBR = string.Empty;
        private ManufacturingTagDto _tagLabel = new ManufacturingTagDto();
        private readonly object _pendingLock = new();
        private readonly HashSet<string> _pendingPids = new(StringComparer.OrdinalIgnoreCase);
        private string _ClientIp;
        public frmTraceAndPrint(IDbContextFactory<MMesDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            InitializeComponent();
            string hostName = Dns.GetHostName();
            // Get IP of client
            _ClientIp = Dns.GetHostAddresses(hostName)
                           .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                           ?.ToString() ?? DateTime.Now.ToString("ffff");
            setPlaceHlderText();
        }

        private void frmTraceAndPrint_Load(object sender, EventArgs e)
        {

        }
        private void setPlaceHlderText()
        {
            this.ActiveControl = txtScanPid;  // txtScan là TextBox của bạn
            txtScanPid.Clear();
            txtScanPid.Focus();
            //textBox1.PlaceholderText = "Escanee la etiqueta aquí para verificar el EBR...";
        }

        private void textBox1_Validated(object sender, EventArgs e)
        {

        }
        string Normalize(string s)
        {
            // 1. Nếu null, rỗng, toàn khoảng trắng → trả về chuỗi rỗng luôn
            if (string.IsNullOrWhiteSpace(s))
                return "";

            // 2. Chuẩn hoá Unicode sang FormKC (Compatibility Composition)
            //    - Ghép lại các ký tự tách rời (O + ́ → Ó)
            //    - Biến các ký tự fullwidth/compatibility thành bản ASCII (ＯＫ → OK)
            var norm = s.Normalize(NormalizationForm.FormKC);

            // 3. Loại bỏ ký tự "khó chịu"
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                if (char.IsControl(ch)) continue;     // bỏ ký tự điều khiển (CR, LF, NULL…)
                //if (char.IsPunctuation(ch)) continue; // bỏ dấu câu/ký tự đặc biệt (. , ! ? …)
                //if (char.IsSymbol(ch)) continue;      // bỏ ký tự dạng symbol (©, ±, ÷, …)
                //if (char.IsPunctuation(ch) && ch != '/') continue; // bỏ dấu câu trừ khi là '/'
                sb.Append(ch);                        // giữ lại chữ cái, số, khoảng trắng bình thường
            }

            // 4. Gom nhiều khoảng trắng liên tiếp thành 1 và Trim 2 đầu
            var cleaned = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();

            // 5. Đưa toàn bộ về in hoa invariant (không phụ thuộc locale)
            return cleaned.ToUpperInvariant();
        }
        private void ApplyStatusStyle(bool kind)
        {

            // Cho label trong suốt để nhìn màu panel
            lbStatus.BackColor = Color.Transparent;

            switch (kind)
            {
                case true:
                    //pnStatus.BackColor = Color.Lime;
                    lbStatus.ForeColor = Color.Black;
                    lbStatus.BackColor = Color.Lime;
                    rtxtDetailExplain.Text = "";

                    break;
                default:
                    //pnStatus.BackColor = Color.Red;
                    lbStatus.ForeColor = Color.Black;
                    lbStatus.BackColor = Color.Red;

                    rtxtDetailExplain.ForeColor = Color.Red;
                    rtxtDetailExplain.BackColor = Color.Yellow;
                    rtxtDetailExplain.Font = new Font(rtxtDetailExplain.Font.FontFamily, 26, FontStyle.Bold);
                    break;
            }
        }
        private void ShowNotice(string msg, bool kind)
        {
            if (kind)
            {
                ApplyStatusStyle(true);
                lbStatus.Text = "OK";
               
                rtxtDetailExplain.Text = "";
            }
            else
            {
                ApplyStatusStyle(false);
                lbStatus.Text = "NG";
                rtxtDetailExplain.Text = $"\u2605 {msg}";
            }

        }
        private async void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter || e.KeyChar == '\r' || e.KeyChar == '\n')
                {
                    string scanned = Normalize(txtScanPid.Text.Trim());
                    setPlaceHlderText();
                    if (!string.IsNullOrEmpty(scanned))
                    {
                        // Lấy thông tin từ db
                        using var db = await _dbFactory.CreateDbContextAsync();

                        var data = await db.TbScanOuts
                          .Where(x => x.Pid == scanned).FirstOrDefaultAsync();
                        if (data == null)
                        {
                            var msg = $"Este código S/N debe escanearse en la aplicación principal.";
                            //var msg = $"No data.";
                            ShowNotice(msg, false);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(_SettingEBR))
                        {
                            _SettingEBR = data.ModelSuffix;
                            UpdateUI(_SettingEBR);
                        }
                        if (!data.PartNo.Equals(_SettingEBR, StringComparison.OrdinalIgnoreCase))
                        {
                            var msg = $"⚠️ P/No escaneado ({data.PartNo}) no coincide con P/No configurado ({_SettingEBR}).";
                            //var msg = $"Wrong PN.";
                            ShowNotice(msg, false);
                            return;
                        }
                        if (data.TagId.HasValue)
                        {
                            var msg = $"Este código S/N [{data.Pid}] debe estar estampado con la etiqueta {data.TagId} en la fecha {data.PrintDate?.ToString("dd/MM/yyyy")}";
                            //var msg = $"Scanned at other Tag.";
                            ShowNotice(msg, false);
                            return;
                        }
                        if (!_pendingPids.Contains(scanned))
                        {
                            lock (_pendingLock)
                            {
                                _pendingPids.Add(scanned);
                            }
                            lstScanedPids.Items.Add(scanned);
                        }
                        ShowNotice(scanned, true);
                        //

                    }

                    e.Handled = true; // Ngăn tiếng "ding" khi nhấn Enter
                }
            }
            catch (Exception ex)
            {

                ShowNotice(ex.Message, false);
            }
            // Nếu scanner gửi Enter (\r hoặc \n)

        }
        #region Print Setting & Generate label tag
        // offset cấu hình (mm). dương = dịch xuống / dịch sang phải
        float _offsetXmm = 0.0f;   // ví dụ 1.5f để dịch phải 1.5mm
        float _offsetYmm = 1.5f;   // ví dụ 1.5f để dịch xuống 1.5mm

        // đổi mm -> hundredths of an inch (đơn vị của MarginBounds)
        static int MmToHundredths(float mm) => (int)Math.Round(mm / 25.4f * 100f);

        void PrintTag(ManufacturingTagDto tag)
        {
            try
            {
                AddLog("PrintTag: Start printing tag.");
                var doc = new PrintDocument();
                doc.PrinterSettings = new PrinterSettings();

                // lấy đúng size ~55x38mm từ driver (217x150 hundredths-inch)
                var ps = doc.PrinterSettings.PaperSizes
                    .Cast<PaperSize>()
                    .FirstOrDefault(p => (Math.Abs(p.Width - 217) < 5 && Math.Abs(p.Height - 150) < 5)
                                      || (Math.Abs(p.Width - 150) < 5 && Math.Abs(p.Height - 217) < 5));
                if (ps != null) doc.DefaultPageSettings.PaperSize = ps;

                doc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
                doc.OriginAtMargins = true;              // (0,0) = MarginBounds.TopLeft
                doc.DefaultPageSettings.Landscape = false; // nếu bị xoay hãy thử true
                AddLog("PrintTag: Start generate label...");
                _tagLabel = tag;
                doc.PrintPage += GenerateLabel_FitMarginBounds;

                AddLog("PrintTag: Calling doc.Print()...");
                doc.Print();
                AddLog("PrintTag: Print completed.");
            }
            catch (Exception ex)
            {
                AddLog($"PrintTag: Exception - {ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
        }

        private void AddLog(string msg)
        {
        }
        void GenerateLabel_FitMarginBounds(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            try
            {
                // safety
                if (_tagLabel == null)
                {
                    e.HasMorePages = false;
                    return;
                }

                // device bounds prefer MarginBounds
                Rectangle deviceBounds = e.MarginBounds;
                if (deviceBounds.Width <= 0 || deviceBounds.Height <= 0)
                {
                    deviceBounds = new Rectangle(e.PageBounds.Left + 10, e.PageBounds.Top + 10,
                                                 Math.Min(400, e.PageBounds.Width - 20),
                                                 Math.Min(300, e.PageBounds.Height - 20));
                }

                // apply offsets (in hundredths of inch)
                int offX = MmToHundredths(_offsetXmm);
                int offY = MmToHundredths(_offsetYmm);
                var dest = new Rectangle(deviceBounds.Left + offX, deviceBounds.Top + offY, deviceBounds.Width, deviceBounds.Height);

                if (dest.Width <= 0 || dest.Height <= 0)
                {
                    e.HasMorePages = false;
                    return;
                }

                // DESIGN: 55mm x 38mm => design units in hundredths of inch
                float designW = 217f; // ~55mm
                float designH = 150f; // ~38mm

                // Compute key layout values in design units (same logic as DrawTag_DesignUnits)
                float padY = designH * 0.05f;            // top/bottom pad used in DrawTag_DesignUnits
                float titleHeight = designH * 0.18f;     // title area height
                                                         // yStart = padY + titleHeight + small gap (0.02*H)
                float yStart = padY + titleHeight + designH * 0.02f;
                // textH = (H - padY) - yStart
                float textH = (designH - padY) - yStart;
                float lineH = textH / 4f;                // 4 lines, last one is Q'TY
                                                         // bottom of Q'TY line sits at: yStart + 3*lineH + lineH = yStart + textH = designH - padY
                float bottomOfQty = designH - padY;

                // small extra gap after Q'TY in mm (how much paper you want after the line)
                float extraGapMm = 0.1f; // tweak this (0.1 mm recommended)
                int extraGapDeviceUnits = MmToHundredths(extraGapMm); // in hundredths of inch (device units)

                // We want to compute required device height to include content up to bottomOfQty + extraGap
                // Choose scale based on width (so width fits design exactly) — avoids unexpected shrink/stretch
                float scaleBasedOnWidth = dest.Width / designW;

                // Required device height to include bottomOfQty:
                float requiredDeviceHeight = bottomOfQty * scaleBasedOnWidth + extraGapDeviceUnits;

                // If device is continuous (dest.Height very large), clamp to requiredDeviceHeight to avoid long feed
                // We'll consider "continuous" when dest.Height >> designH
                float continuousThresholdMultiplier = 3.0f;
                if (dest.Height > designH * continuousThresholdMultiplier)
                {
                    // clamp but ensure at least some minimal height
                    int minAllowed = MmToHundredths(10); // at least 10mm tall as safety floor
                    int clampH = Math.Max(minAllowed, (int)Math.Ceiling(requiredDeviceHeight));
                    // limit clamp to dest.Height if smaller
                    dest = new Rectangle(dest.Left, dest.Top, dest.Width, Math.Min(dest.Height, clampH));
                    // Recompute scale: prefer width-based scale but ensure not to exceed vertical space
                    float scaleYIfNeeded = (float)dest.Height / designH;
                    // final scale chosen should not make content exceed dest: so take min(scaleBasedOnWidth, scaleYIfNeeded)
                    float scale = Math.Min(scaleBasedOnWidth, scaleYIfNeeded);

                    // final drawn sizes
                    float drawnW = designW * scale;
                    float drawnH = bottomOfQty * scale + extraGapDeviceUnits; // we only need height to bottom QTY + gap in device units

                    // compute translate: center horizontally, top align vertically with a small top padding (use offsetYmm)
                    float translateX = dest.Left + (dest.Width - drawnW) / 2f;
                    float topPadding = MmToHundredths(1.0f); // 1mm top padding device units
                    float translateY = dest.Top + topPadding;

                    // Apply transform: note ScaleTransform expects scale relative to design units -> we use 'scale'
                    var state = g.Save();
                    try
                    {
                        g.TranslateTransform(translateX, translateY);
                        g.ScaleTransform(scale, scale);

                        // Draw full design in design coordinates; drawing area will be clipped by dest/driver
                        DrawTag_DesignUnits(g, designW, designH, _tagLabel);
                    }
                    finally
                    {
                        g.Restore(state);
                    }

                    e.HasMorePages = false;
                    return;
                }

                // Non-continuous case (normal page): compute scale uniformly based on min(scaleX, scaleY)
                float scaleX = dest.Width / designW;
                float scaleY = dest.Height / designH;
                float finalScale = Math.Min(scaleX, scaleY);

                float finalDrawnW = designW * finalScale;
                float finalDrawnH = designH * finalScale;

                float translateXNormal = dest.Left + (dest.Width - finalDrawnW) / 2f;
                float topPaddingNormal = MmToHundredths(1.5f);
                float translateYNormal = dest.Top + topPaddingNormal;

                var stateNormal = g.Save();
                try
                {
                    g.TranslateTransform(translateXNormal, translateYNormal);
                    g.ScaleTransform(finalScale, finalScale);

                    DrawTag_DesignUnits(g, designW, designH, _tagLabel);
                }
                finally
                {
                    g.Restore(stateNormal);
                }

                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                AddLog("[ERROR] GenerateLabel_FitMarginBounds Exception: " + ex.ToString());
                e.HasMorePages = false;
            }
        }
        /// <summary>
        /// Vẽ nội dung tem theo "design units" (1/100 inch), ví dụ W≈197, H≈98 cho tem 50x25mm.
        /// Không phụ thuộc DPI vì phần gọi đã scale-fit vào e.MarginBounds.
        /// </summary>
        void DrawTag_DesignUnits(Graphics g, float W, float H, ManufacturingTagDto tag)
        {
            // ———————————————————————————————————————————————————————————
            // 1) SETUP & LAYOUT PARAMETERS
            // ———————————————————————————————————————————————————————————
            using var pen = new Pen(Color.Black, 1);

            // Draw Border (Vẽ viền cho Label)
            g.DrawRectangle(pen, 0, 0, W - 1, H - 1);

            float padX = W * 0.02f;
            float padY = H * 0.05f;

            // Fonts
            using var fTitle = new Font("Segoe UI", 14, FontStyle.Bold, GraphicsUnit.Point);
            using var fBody = new Font("Arial Narrow", 11, FontStyle.Bold, GraphicsUnit.Point);

            // ———————————————————————————————————————————————————————————
            // 2) TITLE: "Manufacture Tag"
            // ———————————————————————————————————————————————————————————
            // Title takes top 15-20%
            var titleRect = new RectangleF(padX, padY, W - padX * 2, H * 0.18f);
            var sfn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.DrawString("Manufacture Tag", fTitle, Brushes.Black, titleRect, sfn);

            // ———————————————————————————————————————————————————————————
            // 3) TEXT BLOCK (4 Lines: Model Name, Date, Part No, Q'TY)
            // ———————————————————————————————————————————————————————————
            float yStart = titleRect.Bottom + H * 0.02f;
            float textH = (H - padY) - yStart;
            float lineH = textH / 4.0f; // 4 equal lines
            float textW = W - padX * 2; // Full width since no QR

            // Helper to draw fit text
            void DrawLine(string text, Font font, int index)
            {
                float y = yStart + index * lineH;
                var rect = new RectangleF(padX, y, textW, lineH);
                // Left align for body text
                var sfnBody = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                // Using DrawFitText if available, or standard DrawString
                DrawFitText(g, text, font, Brushes.Black, rect, 6.0f);
            }

            // Line 1: Model Name
            DrawLine($"Model Name {tag?.ModelName ?? ""}", fBody, 0);

            // Line 2: Date
            DrawLine($"Date {tag?.Date ?? ""}", fBody, 1);

            // Line 3: Part No
            DrawLine($"Part No {tag?.PartNo ?? ""}", fBody, 2);

            // Line 4: Q'TY
            DrawLine($"Q'TY {tag?.Quantity.ToString() ?? ""}", fBody, 3);
        }

        /// <summary>
        /// Vẽ text vào khung và TỰ GIẢM cỡ chữ (Point) để vừa chiều cao.
        /// Không wrap. Nếu vẫn không vừa ở cỡ tối thiểu -> vẽ với ellipsis.
        /// </summary>
        /// <param name="g">Graphics hiện tại</param>
        /// <param name="text">Chuỗi cần vẽ</param>
        /// <param name="baseFont">Font gốc (lấy họ chữ & style)</param>
        /// <param name="brush">Màu bút</param>
        /// <param name="rect">Khung cần vẽ</param>
        /// <param name="minPointSize">Cỡ chữ nhỏ nhất cho phép</param>
        static void DrawFitText(Graphics g, string text, Font baseFont, Brush brush, RectangleF rect, float minPointSize = 6.0f)
        {
            // Không xuống dòng; nếu dài quá sẽ xử lý bằng cách giảm cỡ chữ/ellipsis
            var fmt = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            // Thử giảm dần từ cỡ hiện tại đến min
            for (float sz = baseFont.Size; sz >= minPointSize; sz -= 0.5f)
            {
                using var f = new Font(baseFont.FontFamily, sz, baseFont.Style, GraphicsUnit.Point);
                // MeasureString với width giới hạn để ước lượng có vừa chiều cao không
                var measured = g.MeasureString(text, f, (int)Math.Ceiling(rect.Width), fmt);

                if (measured.Height <= rect.Height + 0.5f)
                {
                    g.DrawString(text, f, brush, rect, fmt);
                    return;
                }
            }

            // Fallback: cỡ min + ellipsis
            using var fMin = new Font(baseFont.FontFamily, minPointSize, baseFont.Style, GraphicsUnit.Point);
            g.DrawString(text, fMin, brush, rect, fmt);
        }

        /// <summary>
        /// Định dạng EBR để hiển thị: 
        /// "EBR12345678" → "EBR 1234 5678"
        /// Nếu không đúng pattern thì giữ nguyên.
        /// </summary>
        static string FormatEbr(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            input = input.Trim().ToUpperInvariant();

            // Nếu bắt đầu bằng EBR và có ít nhất 9 ký tự
            if (input.StartsWith("EBR") && input.Length > 7)
            {
                string prefix = input.Substring(0, 3); // "EBR"
                string rest = input.Substring(3);

                // Cắt thành block 4-4
                if (rest.Length >= 8)
                {
                    string part1 = rest.Substring(0, 4);
                    string part2 = rest.Substring(4, Math.Min(4, rest.Length - 4));
                    return $"{prefix} {part1} {part2}";
                }
                else if (rest.Length >= 4)
                {
                    string part1 = rest.Substring(0, 4);
                    string part2 = rest.Substring(4);
                    return $"{prefix} {part1} {part2}";
                }
            }

            return input;
        }


        private Bitmap GenerateQRCode(string value, int preferDpi = 300) //Qrcode
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(string.IsNullOrEmpty(value) ? "-" : value,
                                        QRCodeGenerator.ECCLevel.M);

            var qrCode = new QRCode(data);
            var bmp = qrCode.GetGraphic(8, Color.Black, Color.White, true);
            bmp.SetResolution(preferDpi, preferDpi);
            return bmp;
        }
        #endregion
        string IpToNumericString(string ip)
        {
            return string.Join("", ip.Split('.').Select(b => int.Parse(b).ToString("D3")));
        }
        long GenerateTagId()
        {
            DateTime now = DateTime.Now;

            string ipNum = IpToNumericString(_ClientIp);
            // timestamp yyMMddHHmmssffff
            string timestamp = now.ToString("yyMMddHHmmssfff");
            var last6Digits = ipNum.Substring(ipNum.Length - 3);
            // Ghép lại
            string tagIdStr = last6Digits + timestamp;
            long tagIdValue = long.Parse(tagIdStr);

            return tagIdValue;
        }
        private async void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (!lbStatus.Text.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = "El último PID escaneado debe ser OK y coincidir con EBR.";
                    //var msg = $"Last PID scan must be OK";
                    ShowNotice(msg, false);
                    txtScanPid.Focus();
                    return;
                }

                // snapshot danh sách PID đang chờ in (batch hiện tại)
                string[] batchPids;
                lock (_pendingLock)
                {
                    batchPids = _pendingPids.ToArray();
                }

                if (batchPids.Length == 0)
                {
                    var msg = "No hay PIDs pendientes para imprimir.R.";
                    //var msg = $"No pid list";
                    ShowNotice(msg, false);
                    txtScanPid.Focus();
                    return;
                }

                //AddLog("Start query (batch only)");

                using var db = await _dbFactory.CreateDbContextAsync();

                // Lấy đúng các dòng thuộc batch hiện tại, chưa in
                //var tagDatas = await db.TbScanOuts
                //    .Where(x => x.PartNo == _SettingEBR
                //                && !x.TagId.HasValue
                //                && batchPids.Contains(x.Pid))
                //    .ToListAsync();
                var tagDatas = await (
                     from s in db.TbScanOuts
                     join m in db.TbModelDicts on s.PartNo equals m.PartNo into gj
                     from m in gj.DefaultIfEmpty()     // <-- left join
                     where batchPids.Contains(s.Pid)
                     select new
                     {
                         PartNo = m != null ? m.PartNo : null,   // có thể null nếu không match
                         ModelName = m != null ? m.ModelName : null,
                         Board = m != null ? m.Board : null,
                         Pid = s.Pid
                     }
                 ).ToListAsync();
                if (tagDatas.Count == 0)
                {
                    ShowNotice("No se encontraron PIDs válidos en el lote actual.", false);
                    txtScanPid.Focus();
                    //// loại các PID không còn match khỏi batch để tránh lặp vô hạn
                    //lock (_pendingLock)
                    //{
                    //    foreach (var pid in batchPids) _pendingPids.Remove(pid);
                    //}

                    return;
                }

                //AddLog("Gen TagID");
                var now = DateTime.Now;
                long tagId = GenerateTagId();

                //AddLog("Assign data for tag label");
                _tagLabel = new ManufacturingTagDto
                {
                    ModelName = $"{tagDatas[0].ModelName}-{tagDatas[0].Board}",
                    Date = now.ToString("dd/MM/yyyy"),
                    PartNo = tagDatas[0].PartNo,
                    Quantity = tagDatas.Count,                // ✅ chỉ đếm batch hiện tại
                    Process = "",
                    TagId = tagId,
                    PidList = tagDatas.Select(x => x.Pid).ToList()
                };

                //AddLog("Print");
                PrintTag(_tagLabel);

                //AddLog("Update DB");
                await db.TbScanOuts
                    .Where(x => _tagLabel.PidList.Contains(x.Pid))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.TagId, r => tagId)
                        .SetProperty(r => r.PrintAt, r => DateTime.Now)
                        .SetProperty(r => r.PrintDate, r => DateOnly.FromDateTime(DateTime.Now))
                    );

                // Xóa đúng các PID vừa in khỏi batch
                lock (_pendingLock)
                {
                    foreach (var pid in _tagLabel.PidList)
                        _pendingPids.Remove(pid);
                }

                // Reset biến/UI
                _tagLabel = new ManufacturingTagDto();
                lstScanedPids.Items.Clear();
                UpdateUI("");
                txtScanPid.Focus();
            }
            catch (Exception ex)
            {
                ShowNotice(ex.Message, false);
            }
        }
        private void UpdateUI(string settingEBR)
        {
            var msg = $"Esta es la interfaz para revisar el EBR e imprimir la etiqueta. ({settingEBR})";
            lbModelInfor.Text = msg;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _tagLabel = new ManufacturingTagDto();
            lstScanedPids.Items.Clear();
            UpdateUI("");
            lock (_pendingLock)
            {
                foreach (var pid in _tagLabel.PidList)
                    _pendingPids.Remove(pid);
            }
            _SettingEBR = string.Empty;

            pnStatus.BackColor = Color.Black;
            lbStatus.ForeColor = Color.Yellow;
            lbStatus.BackColor = Color.Black;
            lbStatus.Text = "LISTA";
            rtxtDetailExplain.Text = "";
            txtScanPid.Focus();
        }
    }
}
