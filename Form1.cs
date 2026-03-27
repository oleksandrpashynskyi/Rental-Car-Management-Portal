using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RentalCars
{
    public partial class Form1 : Form
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["RentalsDb"].ConnectionString;

        // UI fields
        private RadioButton rbCompact, rbSUV, rbSports;
        private DateTimePicker dtRent, dtReturn;
        private CheckBox chkExtraDriver;
        private TextBox txtPlate;
        private Button btnPlaceRental, btnFill, btnUpdate;
        private DataGridView dgv;

        // Keep a DataTable around (useful later for Assignment 3)
        private DataTable _gridTable;

        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;  // crisp on high-DPI
            BuildUi();
            WireEvents();
            EnsureSchemaForAssignment2();
        }

        // ===================== UI (responsive & centered) =====================
        private void BuildUi()
        {
            Text = "Car Rental Company";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(960, 620);

            Controls.Clear();

            // Root: 1 column, 2 rows (Header, Body)
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = new Label
            {
                Text = "Car Rental Company",
                Dock = DockStyle.Fill,
                Height = 56,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20f, FontStyle.Regular)
            };
            root.Controls.Add(header, 0, 0);

            // Body: left rail (Fill/Update) + main content
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // left rail fixed
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.Controls.Add(body, 0, 1);

            // Left rail
            var leftRail = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10, 20, 10, 0)
            };
            btnFill = new Button { Text = "Fill", Width = 80, Height = 28, Margin = new Padding(0, 0, 0, 8) };
            btnUpdate = new Button { Text = "Update", Width = 80, Height = 28 };
            leftRail.Controls.Add(btnFill);
            leftRail.Controls.Add(btnUpdate);
            body.Controls.Add(leftRail, 0, 0);

            // Main area: controls up top, grid below
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.Controls.Add(main, 1, 0);

            // Controls block: 2 columns (car type | dates+checkbox) + centered action row
            var controlsBlock = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10),
                AutoSize = true
            };
            controlsBlock.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            controlsBlock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            main.Controls.Add(controlsBlock, 0, 0);

            // Car type group (left)
            var grpCar = new GroupBox
            {
                Text = "",
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 24, 0),
                AutoSize = true
            };
            rbCompact = new RadioButton { Text = "Compact ($25/day)", AutoSize = true, Checked = true, Margin = new Padding(4) };
            rbSUV = new RadioButton { Text = "SUV ($40/day)", AutoSize = true, Margin = new Padding(4) };
            rbSports = new RadioButton { Text = "Sports Car($60/day)", AutoSize = true, Margin = new Padding(4) };

            var carStack = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
            carStack.Controls.Add(rbCompact);
            carStack.Controls.Add(rbSUV);
            carStack.Controls.Add(rbSports);
            grpCar.Controls.Add(carStack);
            controlsBlock.Controls.Add(grpCar, 0, 0);

            // Dates + extra driver (right)
            var dates = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 2,
                AutoSize = true
            };
            dates.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dates.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dates.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lblRent = new Label { Text = "Rent Date:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 8, 6) };
            dtRent = new DateTimePicker { Format = DateTimePickerFormat.Long, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            chkExtraDriver = new CheckBox { Text = "ExtraDriver Charge ($25)", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(10, 2, 0, 2) };

            var lblReturn = new Label { Text = "Return Date:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 8, 6) };
            dtReturn = new DateTimePicker { Format = DateTimePickerFormat.Long, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };

            dates.Controls.Add(lblRent, 0, 0);
            dates.Controls.Add(dtRent, 1, 0);
            dates.Controls.Add(chkExtraDriver, 2, 0);
            dates.Controls.Add(lblReturn, 0, 1);
            dates.Controls.Add(dtReturn, 1, 1);
            controlsBlock.Controls.Add(dates, 1, 0);

            // Centered action row: Place Rental + License Plate
            var centerRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            centerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            centerRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            centerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var actionPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false
            };
            btnPlaceRental = new Button { Text = "PLACE RENTAL", AutoSize = true, Height = 32, Margin = new Padding(0, 0, 16, 0) };
            var lblPlate = new Label { Text = "License Plate", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 6, 0) };
            txtPlate = new TextBox { Width = 160 };

            actionPanel.Controls.Add(btnPlaceRental);
            actionPanel.Controls.Add(lblPlate);
            actionPanel.Controls.Add(txtPlate);

            centerRow.Controls.Add(new Panel(), 0, 0);     // spacer
            centerRow.Controls.Add(actionPanel, 1, 0);     // centered content
            centerRow.Controls.Add(new Panel(), 2, 0);     // spacer

            controlsBlock.Controls.Add(centerRow, 0, 1);
            controlsBlock.SetColumnSpan(centerRow, 2);

            // Data grid (fills remaining space)
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true, // Assignment 2 = read-only; enable edits for Assignment 3
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            main.Controls.Add(dgv, 0, 1);

            // Initial date constraints
            dtRent.Value = DateTime.Today;
            dtReturn.Value = DateTime.Today.AddDays(1);
            dtReturn.MinDate = dtRent.Value.AddDays(1);
        }

        private void WireEvents()
        {
            dtRent.ValueChanged += (s, e) =>
            {
                dtReturn.MinDate = dtRent.Value.Date.AddDays(1);
                if (dtReturn.Value.Date <= dtRent.Value.Date)
                    dtReturn.Value = dtRent.Value.Date.AddDays(1);
            };

            btnPlaceRental.Click += btnPlaceRental_Click;
            btnFill.Click += btnFill_Click;
            btnUpdate.Click += (s, e) =>
            {
                // Assignment 3 will implement database UPDATEs from the grid.
                MessageBox.Show("Update is part of Assignment 3. For now, use Place Rental and Fill.");
            };

            // ENTER in plate box triggers Place Rental
            txtPlate.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnPlaceRental.PerformClick();
                }
            };
        }

        // ===================== Logic (Assignment 2) =====================
        private (string carType, decimal dailyRate) GetSelection()
        {
            if (rbCompact.Checked) return ("Compact", 25m);
            if (rbSUV.Checked) return ("SUV", 40m);
            return ("Sports Car", 60m);
        }

        private void btnPlaceRental_Click(object sender, EventArgs e)
        {
            string plate = txtPlate.Text.Trim();
            if (string.IsNullOrWhiteSpace(plate))
            {
                MessageBox.Show("Please enter the license plate.");
                txtPlate.Focus();
                return;
            }

            DateTime rentDate = dtRent.Value.Date;
            DateTime returnDate = dtReturn.Value.Date;
            int days = (int)(returnDate - rentDate).TotalDays;
            if (days < 1)
            {
                MessageBox.Show("Return date must be at least one day after the rent date.");
                return;
            }

            var (carType, dailyRate) = GetSelection();
            decimal extraDriverFee = chkExtraDriver.Checked ? 25m : 0m; // flat fee
            decimal subtotal = dailyRate * days + extraDriverFee;
            decimal tax = 0m;  // professor’s spec/UI: no tax shown
            decimal total = subtotal;
            decimal extraDriverPerDayCompat = 0m; // legacy schema: per-day fee not used now

            string sql = @"
INSERT INTO dbo.Rentals
(Plate, CarType, Days, ExtraDriver, RentDate, ReturnDate, DailyRate, ExtraDriverPerDay, ExtraDriverFee, Subtotal, Tax, Total, CreatedAt)
VALUES
(@Plate, @CarType, @Days, @ExtraDriver, @RentDate, @ReturnDate, @DailyRate, @ExtraDriverPerDay, @ExtraDriverFee, @Subtotal, @Tax, @Total, SYSUTCDATETIME());";


            try
            {
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Plate", plate);
                    cmd.Parameters.AddWithValue("@CarType", carType);
                    cmd.Parameters.AddWithValue("@Days", days);
                    cmd.Parameters.AddWithValue("@ExtraDriver", chkExtraDriver.Checked);
                    cmd.Parameters.AddWithValue("@RentDate", rentDate);
                    cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                    cmd.Parameters.AddWithValue("@DailyRate", dailyRate);
                    cmd.Parameters.AddWithValue("@ExtraDriverFee", extraDriverFee);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Tax", tax);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@ExtraDriverPerDay", extraDriverPerDayCompat);


                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    MessageBox.Show(rows == 1 ? "Rental saved." : "Nothing inserted—check table/columns.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Insert failed: " + ex.Message);
            }
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            // Show the exact columns the professor’s grid expects
            string sql = @"
SELECT
    Plate                               AS [Plate#],
    CarType                             AS [Car Type],
    CASE WHEN ExtraDriver=1 THEN 'y' ELSE 'n' END AS [Extra Driver (y/n)],
    CONVERT(date, RentDate)             AS [Rent Date],
    CONVERT(date, ReturnDate)           AS [Return Date],
    CAST(Total AS decimal(10,2))        AS [Total Cost]
FROM dbo.Rentals
ORDER BY RentalID DESC;";
            try
            {
                using (var conn = new SqlConnection(_connStr))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    _gridTable = new DataTable();
                    da.Fill(_gridTable);
                    dgv.AutoGenerateColumns = true;
                    dgv.DataSource = _gridTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fill failed: " + ex.Message);
            }
        }

        // ===================== Non-destructive schema helper =====================
        private void EnsureSchemaForAssignment2()
        {
            string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Rentals' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Rentals
    (
        RentalID          INT IDENTITY(1,1) PRIMARY KEY,
        Plate             NVARCHAR(20) NOT NULL,
        CarType           NVARCHAR(20) NOT NULL,
        Days              INT NOT NULL CHECK (Days > 0),
        ExtraDriver       BIT NOT NULL,
        RentDate          DATE NULL,
        ReturnDate        DATE NULL,
        DailyRate         DECIMAL(10,2) NOT NULL,
        ExtraDriverFee    DECIMAL(10,2) NOT NULL DEFAULT(0),
        Subtotal          DECIMAL(10,2) NOT NULL,
        Tax               DECIMAL(10,2) NOT NULL,
        Total             DECIMAL(10,2) NOT NULL,
        CreatedAt         DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Rentals','RentDate') IS NULL
        ALTER TABLE dbo.Rentals ADD RentDate DATE NULL;

    IF COL_LENGTH('dbo.Rentals','ReturnDate') IS NULL
        ALTER TABLE dbo.Rentals ADD ReturnDate DATE NULL;

    IF COL_LENGTH('dbo.Rentals','ExtraDriverFee') IS NULL
        ALTER TABLE dbo.Rentals ADD ExtraDriverFee DECIMAL(10,2)
            CONSTRAINT DF_Rentals_ExtraDriverFee DEFAULT(0) WITH VALUES;

    IF COL_LENGTH('dbo.Rentals','DailyRate') IS NULL
        ALTER TABLE dbo.Rentals ADD DailyRate DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Rentals_DailyRate DEFAULT(0) WITH VALUES;

    IF COL_LENGTH('dbo.Rentals','Subtotal') IS NULL
        ALTER TABLE dbo.Rentals ADD Subtotal DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Rentals_Subtotal DEFAULT(0) WITH VALUES;

    IF COL_LENGTH('dbo.Rentals','Tax') IS NULL
        ALTER TABLE dbo.Rentals ADD Tax DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Rentals_Tax DEFAULT(0) WITH VALUES;

    IF COL_LENGTH('dbo.Rentals','Total') IS NULL
        ALTER TABLE dbo.Rentals ADD Total DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Rentals_Total DEFAULT(0) WITH VALUES;

    IF COL_LENGTH('dbo.Rentals','CreatedAt') IS NULL
        ALTER TABLE dbo.Rentals ADD CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Rentals_CreatedAt DEFAULT SYSUTCDATETIME() WITH VALUES;
END
";
            try
            {
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Schema check failed. Verify your 'RentalsDb' connection string. Details: " + ex.Message);
            }
        }
    }
}
