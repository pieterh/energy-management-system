﻿// <auto-generated />
using System;
using EMS.DataStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EMS.DataStore.Migrations
{
    [DbContext(typeof(HEMSContext))]
    [Migration("20230615221809_ChargingTransactionStartEnd")]
    partial class ChargingTransactionStartEnd
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("EMS.DataStore.ChargingTransaction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Cost")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("End")
                        .HasColumnType("TEXT");

                    b.Property<double>("EnergyDelivered")
                        .HasColumnType("REAL");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("Start")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("ChargingTransactions");
                });

            modelBuilder.Entity("EMS.DataStore.CostDetail", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChargingTransactionID")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Cost")
                        .HasColumnType("REAL");

                    b.Property<double>("EnergyDelivered")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("TarifStart")
                        .HasColumnType("TEXT");

                    b.Property<double>("TarifUsage")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChargingTransactionID");

                    b.ToTable("CostDetail");
                });

            modelBuilder.Entity("EMS.DataStore.User", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastLogonDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastPasswordChangedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EMS.DataStore.CostDetail", b =>
                {
                    b.HasOne("EMS.DataStore.ChargingTransaction", null)
                        .WithMany("CostDetails")
                        .HasForeignKey("ChargingTransactionID");
                });

            modelBuilder.Entity("EMS.DataStore.ChargingTransaction", b =>
                {
                    b.Navigation("CostDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
