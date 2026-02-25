using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Models;

namespace ProyectoSyncro.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<MetaColumna> MetaColumnas { get; set; }

    public virtual DbSet<MetaOpcione> MetaOpciones { get; set; }

    public virtual DbSet<MetaTabla> MetaTablas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<UsuarioAux> UsuariosAux { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresas__5EF4033E0B839705");

            entity.HasIndex(e => e.NombreSchema, "UQ__Empresas__2564AF9CB6D0E07E").IsUnique();

            entity.HasIndex(e => e.Cifempresa, "UQ__Empresas__F52EBE5DCC4954AB").IsUnique();

            entity.Property(e => e.IdEmpresa).ValueGeneratedNever();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Cifempresa)
                .HasMaxLength(10)
                .HasColumnName("CIFEmpresa");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreEmpresa).HasMaxLength(100);
            entity.Property(e => e.NombreSchema).HasMaxLength(100);
        });

        modelBuilder.Entity<MetaColumna>(entity =>
        {
            entity.HasKey(e => e.IdColumna).HasName("PK__Meta_Col__31452615BC3CF373");

            entity.ToTable("Meta_Columnas");

            entity.HasIndex(e => new { e.IdTabla, e.Nombre }, "UQ_Columna_Tabla").IsUnique();

            entity.Property(e => e.IdColumna).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.TipoDato).HasMaxLength(50);

            entity.HasOne(d => d.IdTablaNavigation).WithMany(p => p.MetaColumnaIdTablaNavigations)
                .HasForeignKey(d => d.IdTabla)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MetaColumnas_MetaTablas");

            entity.HasOne(d => d.IdTablaRelacionadaNavigation).WithMany(p => p.MetaColumnaIdTablaRelacionadaNavigations)
                .HasForeignKey(d => d.IdTablaRelacionada)
                .HasConstraintName("FK_MetaColumnas_TablaRelacionada");
        });

        modelBuilder.Entity<MetaOpcione>(entity =>
        {
            entity.HasKey(e => e.IdOpcion).HasName("PK__Meta_Opc__4F238858A2387F5B");

            entity.ToTable("Meta_Opciones");

            entity.HasIndex(e => new { e.IdColumna, e.Valor }, "UQ_Opcion_Columna").IsUnique();

            entity.Property(e => e.IdOpcion).ValueGeneratedNever();
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Valor).HasMaxLength(100);

            entity.HasOne(d => d.IdColumnaNavigation).WithMany(p => p.MetaOpciones)
                .HasForeignKey(d => d.IdColumna)
                .HasConstraintName("FK_MetaOpciones_Columnas");
        });

        modelBuilder.Entity<MetaTabla>(entity =>
        {
            entity.HasKey(e => e.IdTabla).HasName("PK__Meta_Tab__175D4A58BF24E91C");

            entity.ToTable("Meta_Tablas");

            entity.HasIndex(e => new { e.IdEmpresa, e.Nombre }, "UQ_Tabla_Empresa").IsUnique();

            entity.Property(e => e.IdTabla).ValueGeneratedNever();
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NombreInterno).HasMaxLength(100);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.MetaTablas)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MetaTablas_Empresas");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF97330D74CF");

            entity.HasIndex(e => e.Email, "UQ_Usuario_Email").IsUnique();

            entity.Property(e => e.IdUsuario).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Empresas");
        });

        modelBuilder.Entity<UsuarioAux>(entity =>
        {
            entity.ToTable("UsuariosAux");

            entity.HasKey(e => e.IdUsuario)
                .HasName("PK__UsuariosAux");

            entity.Property(e => e.IdUsuario)
                .ValueGeneratedNever();

            entity.Property(e => e.Salt)
                .HasColumnName("salt")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Password)
                .HasColumnName("password")
                .HasColumnType("varbinary(max)");

            entity.HasOne(d => d.Usuario)
                .WithOne(p => p.UsuarioAux)
                .HasForeignKey<UsuarioAux>(d => d.IdUsuario)
                .HasConstraintName("FK_UsuariosAux_Usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
