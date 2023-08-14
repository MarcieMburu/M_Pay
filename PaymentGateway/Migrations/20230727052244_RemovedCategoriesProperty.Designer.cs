﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentGateway.Data;

#nullable disable

namespace PaymentGateway.Migrations
{
    [DbContext(typeof(PaymentGatewayContext))]
    [Migration("20230727052244_RemovedCategoriesProperty")]
    partial class RemovedCategoriesProperty
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PaymentGateway.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Amount")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("PaymentGateway.Models.Transaction", b =>
                {
                    b.OwnsOne("PaymentGateway.Models.Receiver", "Receiver", b1 =>
                        {
                            b1.Property<int>("TransactionId")
                                .HasColumnType("int");

                            b1.Property<string>("Dst_Account")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("ID_NO")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Phone_No")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("TransactionId");

                            b1.ToTable("Transaction");

                            b1.WithOwner()
                                .HasForeignKey("TransactionId");
                        });

                    b.OwnsOne("PaymentGateway.Models.Sender", "Sender", b1 =>
                        {
                            b1.Property<int>("TransactionId")
                                .HasColumnType("int");

                            b1.Property<string>("ID_NO")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Phone_No")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Src_Account")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("TransactionId");

                            b1.ToTable("Transaction");

                            b1.WithOwner()
                                .HasForeignKey("TransactionId");
                        });

                    b.Navigation("Receiver")
                        .IsRequired();

                    b.Navigation("Sender")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
