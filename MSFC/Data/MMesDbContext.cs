using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MSFC.Models;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using System;
using System.Collections.Generic;

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

    public virtual DbSet<TbImportCursor> TbImportCursors { get; set; }

    public virtual DbSet<TbModelDict> TbModelDicts { get; set; }

    public virtual DbSet<TbScanOut> TbScanOuts { get; set; }

    public virtual DbSet<TbTagTemp> TbTagTemps { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    //=> optionsBuilder.UseMySql("server=10.195.87.245;user=scott;password=ivihaengsung@1;database=mex_mes", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.1.0-mysql"));
    //=> optionsBuilder.UseMySql("server=localhost;user=root;password=root;database=mex_mes", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.42-mysql"));
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // fallback nếu DI chưa cấu hình, đọc file cấu hình thủ công
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(9, 1, 0));

            //var conn = config.GetConnectionString("LocalConnection");
            //var serverVersion = new MySqlServerVersion(new Version(8, 0, 42));

            optionsBuilder.UseMySql(conn, serverVersion);
        }
    }
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

        modelBuilder.Entity<TbImportCursor>(entity =>
        {
            entity.HasKey(e => e.LineKey).HasName("PRIMARY");

            entity.ToTable("tb_import_cursor");

            entity.Property(e => e.LineKey).HasMaxLength(45);
            entity.Property(e => e.LastFileName).HasMaxLength(255);
            entity.Property(e => e.LastStampUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<TbModelDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_model_dict");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Board)
                .HasMaxLength(45)
                .HasColumnName("BOARD");
            entity.Property(e => e.ModelName)
                .HasMaxLength(45)
                .HasColumnName("MODEL_NAME");
            entity.Property(e => e.PartNo)
                .HasMaxLength(45)
                .HasColumnName("PART_NO");
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

        modelBuilder.Entity<TbTagTemp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_tag_temp");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Client)
                .HasMaxLength(45)
                .HasColumnName("CLIENT");
            entity.Property(e => e.Model)
                .HasMaxLength(45)
                .HasColumnName("MODEL");
            entity.Property(e => e.PartNo)
                .HasMaxLength(45)
                .HasColumnName("PART_NO");
            entity.Property(e => e.Pid)
                .HasMaxLength(45)
                .HasColumnName("PID");
            entity.Property(e => e.ScanAt)
                .HasColumnType("datetime")
                .HasColumnName("SCAN_AT");
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(45)
                .HasColumnName("WORK_ORDER");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active)
                .HasMaxLength(45)
                .HasColumnName("ACTIVE");
            entity.Property(e => e.Dept)
                .HasMaxLength(45)
                .HasColumnName("DEPT");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Factory)
                .HasMaxLength(45)
                .HasColumnName("FACTORY");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("NAME");
            entity.Property(e => e.Password)
                .HasMaxLength(45)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.UserName)
                .HasMaxLength(45)
                .HasColumnName("USER_NAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
