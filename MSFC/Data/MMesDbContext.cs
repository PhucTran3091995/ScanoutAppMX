using System;
using System.Collections.Generic;
using MSFC.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MSFC.Data;

public partial class MMesDbContext : DbContext
{
    public MMesDbContext()
    {
    }

    public MMesDbContext(DbContextOptions<MMesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAoiNg> TbAoiNgs { get; set; }

    public virtual DbSet<TbAoiPid> TbAoiPids { get; set; }

    public virtual DbSet<TbBlock> TbBlocks { get; set; }

    public virtual DbSet<TbDefectImage> TbDefectImages { get; set; }

    public virtual DbSet<TbDept> TbDepts { get; set; }

    public virtual DbSet<TbImportCursor> TbImportCursors { get; set; }

    public virtual DbSet<TbInternalDefect> TbInternalDefects { get; set; }

    public virtual DbSet<TbKla> TbKlas { get; set; }

    public virtual DbSet<TbModelDict> TbModelDicts { get; set; }

    public virtual DbSet<TbProductionPlan> TbProductionPlans { get; set; }

    public virtual DbSet<TbRomColor> TbRomColors { get; set; }

    public virtual DbSet<TbRomDatum> TbRomData { get; set; }

    public virtual DbSet<TbRomHistory> TbRomHistories { get; set; }

    public virtual DbSet<TbScanOut> TbScanOuts { get; set; }

    public virtual DbSet<TbStationMonitoring> TbStationMonitorings { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    //=> optionsBuilder.UseMySql("server=192.168.0.4;user=scott;password=ivihaengsung@1;database=mex_mes", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.1.0-mysql"));
    //        => optionsBuilder.UseMySql("server=10.195.89.20;user=root;password=ivihaengsung@1;database=mex_mes", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<TbAoiNg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_aoi_ng");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ArrayId)
                .HasMaxLength(45)
                .HasColumnName("ARRAY_ID");
            entity.Property(e => e.ArrayIndex).HasColumnName("ARRAY_INDEX");
            entity.Property(e => e.Component)
                .HasMaxLength(22)
                .HasColumnName("COMPONENT");
            entity.Property(e => e.EndDate).HasColumnName("END_DATE");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("END_TIME");
            entity.Property(e => e.InspType)
                .HasMaxLength(45)
                .HasColumnName("INSP_TYPE");
            entity.Property(e => e.Pid)
                .HasMaxLength(22)
                .HasColumnName("PID");
            entity.Property(e => e.UserResult)
                .HasMaxLength(45)
                .HasColumnName("USER_RESULT");
        });

        modelBuilder.Entity<TbAoiPid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_aoi_pid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArrayId)
                .HasMaxLength(22)
                .HasColumnName("ARRAY_ID");
            entity.Property(e => e.EndDate).HasColumnName("END_DATE");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("END_TIME");
            entity.Property(e => e.Line)
                .HasMaxLength(10)
                .HasColumnName("LINE");
            entity.Property(e => e.MachineResult)
                .HasMaxLength(45)
                .HasColumnName("MACHINE_RESULT");
            entity.Property(e => e.Model)
                .HasMaxLength(45)
                .HasColumnName("MODEL");
            entity.Property(e => e.Pid)
                .HasMaxLength(22)
                .HasColumnName("PID");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(255)
                .HasColumnName("PROGRAM_NAME");
            entity.Property(e => e.UserResult)
                .HasMaxLength(45)
                .HasColumnName("USER_RESULT");
            entity.Property(e => e.WorkFace)
                .HasMaxLength(3)
                .HasColumnName("WORK_FACE");
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(45)
                .HasColumnName("WORK_ORDER");
        });

        modelBuilder.Entity<TbBlock>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity.ToTable("tb_block");

            entity.Property(e => e.Pid)
                .HasMaxLength(22)
                .HasColumnName("pid");
            entity.Property(e => e.BlockAt)
                .HasColumnType("datetime")
                .HasColumnName("BLOCK_AT");
            entity.Property(e => e.History).HasColumnType("text");
            entity.Property(e => e.ReleaseAt)
                .HasColumnType("datetime")
                .HasColumnName("RELEASE_AT");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasComment("B: Block - R: Release")
                .HasColumnName("STATUS");
        });

        modelBuilder.Entity<TbDefectImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_defect_images");

            entity.HasIndex(e => e.DefectId, "fk_defectImage_defectId_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DefectId).HasColumnName("DEFECT_ID");
            entity.Property(e => e.Path)
                .HasMaxLength(255)
                .HasColumnName("path");

            entity.HasOne(d => d.Defect).WithMany(p => p.TbDefectImages)
                .HasForeignKey(d => d.DefectId)
                .HasConstraintName("fk_defectImage_defectId");
        });

        modelBuilder.Entity<TbDept>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_dept");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Authority)
                .HasMaxLength(45)
                .HasColumnName("AUTHORITY");
            entity.Property(e => e.Dept)
                .HasMaxLength(45)
                .HasColumnName("DEPT");
        });

        modelBuilder.Entity<TbImportCursor>(entity =>
        {
            entity.HasKey(e => e.LineKey).HasName("PRIMARY");

            entity.ToTable("tb_import_cursor");

            entity.Property(e => e.LineKey).HasMaxLength(45);
            entity.Property(e => e.LastFileName).HasMaxLength(255);
            entity.Property(e => e.LastStampUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<TbInternalDefect>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_internal_defect");

            entity.HasIndex(e => e.IssueOwner, "fk_internalDefect_issueOwner_idx");

            entity.HasIndex(e => e.RepairMan, "fk_internalDefect_repairMan_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionContent)
                .HasMaxLength(255)
                .HasColumnName("ACTION_CONTENT");
            entity.Property(e => e.ActionDate).HasColumnName("ACTION_DATE");
            entity.Property(e => e.ActionType)
                .HasMaxLength(45)
                .HasColumnName("ACTION_TYPE");
            entity.Property(e => e.Cause)
                .HasMaxLength(255)
                .HasColumnName("CAUSE");
            entity.Property(e => e.DefectDate).HasColumnName("DEFECT_DATE");
            entity.Property(e => e.DefectImage).HasColumnName("DEFECT_IMAGE");
            entity.Property(e => e.DefectName)
                .HasMaxLength(255)
                .HasColumnName("DEFECT_NAME");
            entity.Property(e => e.IssueOwner).HasColumnName("ISSUE_OWNER");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("LOCATION");
            entity.Property(e => e.Pid)
                .HasMaxLength(45)
                .HasColumnName("PID");
            entity.Property(e => e.Process)
                .HasMaxLength(45)
                .HasColumnName("PROCESS");
            entity.Property(e => e.RepairMan).HasColumnName("REPAIR_MAN");
            entity.Property(e => e.Side)
                .HasMaxLength(4)
                .HasColumnName("SIDE");

            entity.HasOne(d => d.IssueOwnerNavigation).WithMany(p => p.TbInternalDefects)
                .HasForeignKey(d => d.IssueOwner)
                .HasConstraintName("fk_internalDefect_issueOwner");

            entity.HasOne(d => d.RepairManNavigation).WithMany(p => p.TbInternalDefects)
                .HasForeignKey(d => d.RepairMan)
                .HasConstraintName("fk_internalDefect_repairMan");
        });

        modelBuilder.Entity<TbKla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_klas");

            entity.HasIndex(e => e.Wo, "WO_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ebr)
                .HasMaxLength(45)
                .HasColumnName("EBR");
            entity.Property(e => e.EndSerial)
                .HasMaxLength(45)
                .HasColumnName("END_SERIAL");
            entity.Property(e => e.EndSn)
                .HasMaxLength(45)
                .HasColumnName("END_SN");
            entity.Property(e => e.StartSerial)
                .HasMaxLength(45)
                .HasColumnName("START_SERIAL");
            entity.Property(e => e.StartSn)
                .HasMaxLength(45)
                .HasColumnName("START_SN");
            entity.Property(e => e.Wo)
                .HasMaxLength(45)
                .HasColumnName("WO");
        });

        modelBuilder.Entity<TbModelDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_model_dict");

            entity.HasIndex(e => e.PartNo, "PART_NO_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Board)
                .HasMaxLength(45)
                .HasColumnName("BOARD");
            entity.Property(e => e.ModelName)
                .HasMaxLength(45)
                .HasColumnName("MODEL_NAME");
            entity.Property(e => e.ModelSuffix)
                .HasMaxLength(45)
                .HasColumnName("MODEL_SUFFIX");
            entity.Property(e => e.PartNo)
                .HasMaxLength(45)
                .HasColumnName("PART_NO");
        });

        modelBuilder.Entity<TbProductionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_production_plan");

            entity.HasIndex(e => new { e.Buyer, e.ModelName, e.BoardName }, "IX_ProductionPlans_Buyer_ModelName_Board");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BarePcbPn)
                .HasMaxLength(11)
                .HasColumnName("BARE_PCB_PN");
            entity.Property(e => e.BoardName)
                .HasMaxLength(50)
                .HasColumnName("BOARD_NAME");
            entity.Property(e => e.Buyer)
                .HasMaxLength(50)
                .HasColumnName("BUYER");
            entity.Property(e => e.GmesWo)
                .HasMaxLength(13)
                .HasColumnName("GMES_WO");
            entity.Property(e => e.Lane)
                .HasMaxLength(2)
                .HasColumnName("LANE");
            entity.Property(e => e.Line)
                .HasMaxLength(2)
                .HasColumnName("LINE");
            entity.Property(e => e.ModelName)
                .HasMaxLength(50)
                .HasColumnName("MODEL_NAME");
            entity.Property(e => e.ModelSuffix)
                .HasMaxLength(50)
                .HasColumnName("MODEL_SUFFIX");
            entity.Property(e => e.PcbaAssyPn)
                .HasMaxLength(11)
                .HasColumnName("PCBA_ASSY_PN");
            entity.Property(e => e.ProdDate).HasColumnName("PROD_DATE");
            entity.Property(e => e.ProdTime)
                .HasColumnType("datetime")
                .HasColumnName("PROD_TIME");
            entity.Property(e => e.SmtAssyPn)
                .HasMaxLength(11)
                .HasColumnName("SMT_ASSY_PN");
            entity.Property(e => e.WoQty).HasColumnName("WO_Qty");
        });

        modelBuilder.Entity<TbRomColor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_rom_color");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(45)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TbRomDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_rom_data");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active)
                .HasMaxLength(1)
                .HasColumnName("ACTIVE");
            entity.Property(e => e.Board)
                .HasMaxLength(11)
                .HasColumnName("BOARD");
            entity.Property(e => e.ChecksumTypeDedi)
                .HasMaxLength(255)
                .HasColumnName("CHECKSUM_TYPE_DEDI");
            entity.Property(e => e.ColorCode1)
                .HasMaxLength(45)
                .HasColumnName("COLOR_CODE1");
            entity.Property(e => e.ColorCode2)
                .HasMaxLength(45)
                .HasColumnName("COLOR_CODE2");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EcoNo)
                .HasMaxLength(255)
                .HasColumnName("ECO_NO");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("FILE_NAME");
            entity.Property(e => e.FirstWo)
                .HasMaxLength(255)
                .HasColumnName("FIRST_WO");
            entity.Property(e => e.HseChecksumDedi)
                .HasMaxLength(255)
                .HasColumnName("HSE_CHECKSUM_DEDI");
            entity.Property(e => e.HseChecksumDitek)
                .HasMaxLength(255)
                .HasColumnName("HSE_CHECKSUM_DITEK");
            entity.Property(e => e.IcMakerPn)
                .HasMaxLength(255)
                .HasColumnName("IC_MAKER_PN");
            entity.Property(e => e.IcName)
                .HasMaxLength(11)
                .HasColumnName("IC_NAME");
            entity.Property(e => e.IcPn)
                .HasMaxLength(255)
                .HasColumnName("IC_PN");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("LAST_UPDATED_DATE");
            entity.Property(e => e.LgChecksum)
                .HasMaxLength(255)
                .HasColumnName("LG_CHECKSUM");
            entity.Property(e => e.MarkingColor1)
                .HasMaxLength(255)
                .HasColumnName("MARKING_COLOR1");
            entity.Property(e => e.MarkingColor2)
                .HasMaxLength(255)
                .HasColumnName("MARKING_COLOR2");
            entity.Property(e => e.MarkingLaser)
                .HasMaxLength(255)
                .HasColumnName("MARKING_LASER");
            entity.Property(e => e.Mc)
                .HasMaxLength(45)
                .HasColumnName("MC");
            entity.Property(e => e.Model)
                .HasMaxLength(15)
                .HasColumnName("MODEL");
            entity.Property(e => e.ModelSuffix)
                .HasMaxLength(45)
                .HasColumnName("MODEL_SUFFIX");
            entity.Property(e => e.PDataDedi)
                .HasMaxLength(255)
                .HasColumnName("P_DATA_DEDI");
            entity.Property(e => e.PackageInfoDedi)
                .HasMaxLength(255)
                .HasColumnName("PACKAGE_INFO_DEDI");
            entity.Property(e => e.PcbPn)
                .HasMaxLength(45)
                .HasColumnName("PCB_PN");
            entity.Property(e => e.PcbaAssyPn)
                .HasMaxLength(11)
                .HasColumnName("PCBA_ASSY_PN");
            entity.Property(e => e.ProgramType)
                .HasMaxLength(255)
                .HasColumnName("PROGRAM_TYPE");
            entity.Property(e => e.ReleaseDate)
                .HasColumnType("datetime")
                .HasColumnName("RELEASE_DATE");
            entity.Property(e => e.ReleaseNoteFile)
                .HasMaxLength(255)
                .HasColumnName("RELEASE_NOTE_FILE");
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .HasColumnName("REMARKS");
            entity.Property(e => e.SmtAssyPn)
                .HasMaxLength(11)
                .HasColumnName("SMT_ASSY_PN");
            entity.Property(e => e.SocketName)
                .HasMaxLength(255)
                .HasColumnName("SOCKET_NAME");
            entity.Property(e => e.Stage)
                .HasMaxLength(45)
                .HasColumnName("STAGE");
            entity.Property(e => e.SwProgramName)
                .HasMaxLength(255)
                .HasColumnName("SW_PROGRAM_NAME");
            entity.Property(e => e.SwVersion)
                .HasMaxLength(45)
                .HasColumnName("SW_VERSION");
        });

        modelBuilder.Entity<TbRomHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_rom_history");

            entity.HasIndex(e => e.ActionBy, "fk_romHis_actionBy_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionAt)
                .HasColumnType("datetime")
                .HasColumnName("action_at");
            entity.Property(e => e.ActionBy).HasColumnName("action_by");
            entity.Property(e => e.ActionContent)
                .HasColumnType("text")
                .HasColumnName("action_content");
            entity.Property(e => e.ActionType)
                .HasMaxLength(45)
                .HasColumnName("action_type");
            entity.Property(e => e.PrintAt)
                .HasColumnType("datetime")
                .HasColumnName("print_at");
            entity.Property(e => e.RomId).HasColumnName("rom_id");

            entity.HasOne(d => d.ActionByNavigation).WithMany(p => p.TbRomHistories)
                .HasForeignKey(d => d.ActionBy)
                .HasConstraintName("fk_romHis_actionBy");
        });

        modelBuilder.Entity<TbScanOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_scan_out", tb => tb.HasComment("table to record scan out pid"));

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ClientId)
                .HasMaxLength(45)
                .HasColumnName("CLIENT_ID");
            entity.Property(e => e.FirstInspector)
                .HasMaxLength(45)
                .HasColumnName("FIRST_INSPECTOR");
            entity.Property(e => e.ModelName)
                .HasMaxLength(45)
                .HasColumnName("MODEL_NAME");
            entity.Property(e => e.ModelSuffix)
                .HasMaxLength(45)
                .HasColumnName("MODEL_SUFFIX");
            entity.Property(e => e.PartNo)
                .HasMaxLength(45)
                .HasColumnName("PART_NO");
            entity.Property(e => e.Pid)
                .HasMaxLength(45)
                .HasColumnName("PID");
            entity.Property(e => e.PrintAt)
                .HasColumnType("datetime")
                .HasColumnName("PRINT_AT");
            entity.Property(e => e.PrintDate).HasColumnName("PRINT_DATE");
            entity.Property(e => e.Qty).HasColumnName("QTY");
            entity.Property(e => e.ScanAt)
                .HasColumnType("datetime")
                .HasColumnName("SCAN_AT");
            entity.Property(e => e.ScanDate).HasColumnName("SCAN_DATE");
            entity.Property(e => e.SecondInspector)
                .HasMaxLength(45)
                .HasColumnName("SECOND_INSPECTOR");
            entity.Property(e => e.TagId).HasColumnName("TAG_ID");
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(45)
                .HasColumnName("WORK_ORDER");
            entity.Property(e => e._4m)
                .HasMaxLength(45)
                .HasColumnName("4M");
        });

        modelBuilder.Entity<TbStationMonitoring>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_station_monitoring");

            entity.HasIndex(e => e.IpAddress, "IP_ADDRESS_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("IP_ADDRESS");
            entity.Property(e => e.Note)
                .HasColumnType("text")
                .HasColumnName("NOTE");
            entity.Property(e => e.StationName)
                .HasMaxLength(45)
                .HasColumnName("STATION_NAME");
            entity.Property(e => e.Status)
                .HasMaxLength(45)
                .HasComment("RUNNING\nOFF")
                .HasColumnName("STATUS");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_AT");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_user");

            entity.HasIndex(e => e.Email, "EMAIL_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UserName, "USER_NAME_UNIQUE").IsUnique();

            entity.HasIndex(e => e.RoleId, "fk_user_role_idx");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active)
                .HasMaxLength(1)
                .HasColumnName("ACTIVE");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATE_AT");
            entity.Property(e => e.Email).HasColumnName("EMAIL");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("FULL_NAME");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("LAST_LOGIN");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.Position)
                .HasMaxLength(255)
                .HasColumnName("POSITION");
            entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_AT");
            entity.Property(e => e.UserName)
                .HasMaxLength(45)
                .HasColumnName("USER_NAME");

            entity.HasOne(d => d.Role).WithMany(p => p.TbUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_user_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
