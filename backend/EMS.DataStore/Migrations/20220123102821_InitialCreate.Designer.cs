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
    [Migration("20220123102821_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("EMS.DataStore.ChargingTransaction", b =>
                {
                    b.Property<int>("ID")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Cost")
                        .HasColumnType("REAL");

                    b.Property<int>("EnergyDelivered")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("ChargingTransactions");
                });
#pragma warning restore 612, 618
        }
    }
}