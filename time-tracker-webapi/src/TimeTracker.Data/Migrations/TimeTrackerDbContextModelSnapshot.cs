﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TimeTracker.Data.Migrations
{
    [DbContext(typeof(TimeTrackerDbContext))]
    partial class TimeTrackerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TimeTracker.Data.Models.BillingClient", b =>
                {
                    b.Property<int>("BillingClientId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AddressLine1");

                    b.Property<string>("AddressLine2");

                    b.Property<string>("CityStateZip");

                    b.Property<string>("Email");

                    b.Property<string>("Name");

                    b.HasKey("BillingClientId");

                    b.ToTable("BillingClients");
                });

            modelBuilder.Entity("TimeTracker.Data.Models.BillingRate", b =>
                {
                    b.Property<int>("BillingRateId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BillingClientId");

                    b.Property<DateTime?>("End");

                    b.Property<int?>("ProjectId");

                    b.Property<DateTime>("Start");

                    b.Property<Guid>("UserId");

                    b.HasKey("BillingRateId");

                    b.ToTable("BillingRates");
                });

            modelBuilder.Entity("TimeTracker.Data.Models.Project", b =>
                {
                    b.Property<int>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BillingClientId");

                    b.Property<DateTime?>("End");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("Start");

                    b.HasKey("ProjectId");

                    b.HasIndex("BillingClientId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("TimeTracker.Data.Models.TimeEntry", b =>
                {
                    b.Property<Guid>("TimeEntryId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BillingClientId");

                    b.Property<DateTime>("Date");

                    b.Property<double>("Hours");

                    b.Property<bool>("IsBillable");

                    b.Property<string>("NonBillableReason");

                    b.Property<int?>("ProjectId");

                    b.Property<int>("TimeEntryType");

                    b.Property<Guid>("UserId");

                    b.HasKey("TimeEntryId");

                    b.HasIndex("BillingClientId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("UserId");

                    b.ToTable("TimeEntries");
                });

            modelBuilder.Entity("TimeTracker.Data.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("SlackUserId");

                    b.Property<string>("UserName");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TimeTracker.Data.Models.Project", b =>
                {
                    b.HasOne("TimeTracker.Data.Models.BillingClient", "BillingClient")
                        .WithMany()
                        .HasForeignKey("BillingClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TimeTracker.Data.Models.TimeEntry", b =>
                {
                    b.HasOne("TimeTracker.Data.Models.BillingClient", "BillingClient")
                        .WithMany()
                        .HasForeignKey("BillingClientId");

                    b.HasOne("TimeTracker.Data.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId");

                    b.HasOne("TimeTracker.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
