using Microsoft.EntityFrameworkCore;
using MSFC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSFC.UC
{
    public partial class Summary : Form
    {
        private readonly IDbContextFactory<MMesDbContext> _dbFactory;
        private readonly string _clientIp;
        public Summary(IDbContextFactory<MMesDbContext> dbFactory, string clientIp)
        {
            _dbFactory = dbFactory;
            _clientIp = clientIp;

            InitializeComponent();

            dtFromTime.Value = DateTime.Today;
            dtToTime.Value = DateTime.Today.AddDays(1).AddSeconds(-1);
        }

        private void Summary_Load(object sender, EventArgs e)
        {
            LoadSummary();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadSummary();
        }

        private void LoadSummary()
        {
            // Kiểm tra hợp lệ (chỉ so sánh ngày để tránh lỗi do giờ)
            if (dtFromTime.Value.Date > dtToTime.Value.Date)
            {
                MessageBox.Show("Khoảng thời gian không hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dbContext = _dbFactory.CreateDbContext();

            // --- CHỈNH SỬA NGÀY GIỜ ---
            // Lấy từ đầu ngày (00:00:00) của dtFromTime
            var from = dtFromTime.Value.Date;
            // Lấy đến cuối ngày (23:59:59) của dtToTime
            var to = dtToTime.Value.Date.AddDays(1).AddSeconds(-1);

            var records = dbContext.TbScanOuts
                .Where(s => s.ScanAt.HasValue)
                .Where(s => s.ClientId == _clientIp)
                .Where(s => s.ScanAt >= from && s.ScanAt <= to)
                .Select(s => new
                {
                    ScanAt = s.ScanAt!.Value,
                    Qty = s.Qty ?? 1,
                    PartNo = s.PartNo // --- LẤY THÊM PART_NO ---
                })
                .AsEnumerable()
                .Select(s => new ScanRecord(s.ScanAt, s.Qty, s.PartNo)) // Truyền PartNo vào record
                .ToList();

            var summaries = BuildSummary(records);
            dvSummary.DataSource = summaries;

            // --- CẤU HÌNH HEADER CỘT ---
            dvSummary.Columns["Date"].HeaderText = "Date";
            dvSummary.Columns["PartNo"].HeaderText = "Part No"; // Header cho cột mới
            dvSummary.Columns["Slot0730To1000"].HeaderText = "07:30 - 10:00";
            dvSummary.Columns["Slot1000To1230"].HeaderText = "10:00 - 12:30";
            dvSummary.Columns["Slot1230To1515"].HeaderText = "12:30 - 15:15";
            dvSummary.Columns["Slot1515To1730"].HeaderText = "15:15 - 17:30";
            dvSummary.Columns["Slot1730To2000"].HeaderText = "17:30 - 20:00";
            dvSummary.Columns["Slot2000To0730"].HeaderText = "20:00 - 07:30";
            dvSummary.Columns["Total"].HeaderText = "Total";

            // Đảm bảo cột PartNo hiển thị ở vị trí mong muốn (ví dụ sau cột Date)
            if (dvSummary.Columns["PartNo"] != null)
                dvSummary.Columns["PartNo"].DisplayIndex = 1;
        }

        private List<SummaryRow> BuildSummary(IEnumerable<ScanRecord> records)
        {
            // Key của Dictionary bây giờ là cặp (DateOnly, PartNo)
            var result = new Dictionary<(DateOnly, string), SummaryRow>();

            foreach (var record in records)
            {
                var slotInfo = GetSlotInfo(record.ScanAt);
                // Xử lý null cho PartNo nếu cần
                string pNo = record.PartNo ?? "";

                // Tạo key tổng hợp
                var key = (slotInfo.Day, pNo);

                if (!result.TryGetValue(key, out var row))
                {
                    row = new SummaryRow(slotInfo.Day, pNo);
                    result[key] = row;
                }

                var qty = record.Qty;
                switch (slotInfo.Slot)
                {
                    case TimeSlot.Slot0730To1000:
                        row.Slot0730To1000 += qty;
                        break;
                    case TimeSlot.Slot1000To1230:
                        row.Slot1000To1230 += qty;
                        break;
                    case TimeSlot.Slot1230To1515:
                        row.Slot1230To1515 += qty;
                        break;
                    case TimeSlot.Slot1515To1730:
                        row.Slot1515To1730 += qty;
                        break;
                    case TimeSlot.Slot1730To2000:
                        row.Slot1730To2000 += qty;
                        break;
                    case TimeSlot.Slot2000To0730:
                        row.Slot2000To0730 += qty;
                        break;
                }
            }

            return result.Values
                .OrderBy(r => r.Date)
                .ThenBy(r => r.PartNo) // Sắp xếp thêm theo PartNo
                .Select(r => r.WithTotal())
                .ToList();
        }

        private (DateOnly Day, TimeSlot Slot) GetSlotInfo(DateTime scanTime)
        {
            var timeOfDay = scanTime.TimeOfDay;
            var slotDay = DateOnly.FromDateTime(scanTime);
            var start0730 = new TimeSpan(7, 30, 0);

            if (timeOfDay < start0730)
            {
                slotDay = slotDay.AddDays(-1);
            }

            if (timeOfDay >= start0730 && timeOfDay < new TimeSpan(10, 0, 0))
                return (slotDay, TimeSlot.Slot0730To1000);
            if (timeOfDay >= new TimeSpan(10, 0, 0) && timeOfDay < new TimeSpan(12, 30, 0))
                return (slotDay, TimeSlot.Slot1000To1230);
            if (timeOfDay >= new TimeSpan(12, 30, 0) && timeOfDay < new TimeSpan(15, 15, 0))
                return (slotDay, TimeSlot.Slot1230To1515);
            if (timeOfDay >= new TimeSpan(15, 15, 0) && timeOfDay < new TimeSpan(17, 30, 0))
                return (slotDay, TimeSlot.Slot1515To1730);
            if (timeOfDay >= new TimeSpan(17, 30, 0) && timeOfDay < new TimeSpan(20, 0, 0))
                return (slotDay, TimeSlot.Slot1730To2000);

            return (slotDay, TimeSlot.Slot2000To0730);
        }

        private enum TimeSlot
        {
            Slot0730To1000,
            Slot1000To1230,
            Slot1230To1515,
            Slot1515To1730,
            Slot1730To2000,
            Slot2000To0730
        }

        private class SummaryRow
        {
            // Sửa constructor để nhận thêm partNo
            public SummaryRow(DateOnly date, string partNo)
            {
                Date = date.ToString("yyyy-MM-dd");
                PartNo = partNo;
            }

            public string Date { get; set; }

            // Thêm property PartNo
            public string PartNo { get; set; }

            public int Slot0730To1000 { get; set; }
            public int Slot1000To1230 { get; set; }
            public int Slot1230To1515 { get; set; }
            public int Slot1515To1730 { get; set; }
            public int Slot1730To2000 { get; set; }
            public int Slot2000To0730 { get; set; }
            public int Total { get; set; }

            public SummaryRow WithTotal()
            {
                Total = Slot0730To1000 + Slot1000To1230 + Slot1230To1515 + Slot1515To1730 + Slot1730To2000 + Slot2000To0730;
                return this;
            }
        }

        private readonly record struct ScanRecord(DateTime ScanAt, int Qty, string PartNo);
    }
}
    

