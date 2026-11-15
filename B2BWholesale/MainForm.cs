using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using B2BWholesale.Data;

namespace B2BWholesale
{
    public class MainForm : Form
    {
        private readonly Database db = new Database();
        private readonly string adminUser;
        private DataGridView dgvItems, dgvAgents, dgvOrders;
        private ComboBox cmbOrderStatus;
        private PrintDocument manifestDocument;
        private DataTable manifestData;
        private int manifestLineIndex;

        public MainForm(string username)
        {
            adminUser = username;
            Text = "B2B Wholesale Management - Admin: " + adminUser;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildItemsTab());
            tabs.TabPages.Add(BuildAgentsTab());
            tabs.TabPages.Add(BuildOrdersTab());
            Controls.Add(tabs);

            manifestDocument = new PrintDocument();
            manifestDocument.DocumentName = "Shipping Manifest";
            manifestDocument.PrintPage += ManifestPrintPage;
            Load += (s, e) => { RefreshItems(); RefreshAgents(); RefreshOrders(); };
        }

        private TabPage BuildItemsTab()
        {
            TabPage page = new TabPage("Items");
            dgvItems = CreateGrid();
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            buttons.Controls.Add(MakeButton("Add Item", (s, e) => AddItem()));
            buttons.Controls.Add(MakeButton("Edit Selected", (s, e) => EditItem()));
            buttons.Controls.Add(MakeButton("Delete Selected", (s, e) => DeleteItem()));
            buttons.Controls.Add(MakeButton("Refresh", (s, e) => RefreshItems()));
            page.Controls.Add(dgvItems);
            page.Controls.Add(buttons);
            return page;
        }

        private TabPage BuildAgentsTab()
        {
            TabPage page = new TabPage("Agents");
            dgvAgents = CreateGrid();
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            buttons.Controls.Add(MakeButton("Add Agent", (s, e) => AddAgent()));
            buttons.Controls.Add(MakeButton("Edit Selected", (s, e) => EditAgent()));
            buttons.Controls.Add(MakeButton("Delete Selected", (s, e) => DeleteAgent()));
            buttons.Controls.Add(MakeButton("Refresh", (s, e) => RefreshAgents()));
            page.Controls.Add(dgvAgents);
            page.Controls.Add(buttons);
            return page;
        }

        private TabPage BuildOrdersTab()
        {
            TabPage page = new TabPage("Incoming Web Orders");
            dgvOrders = CreateGrid();
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            buttons.Controls.Add(new Label { Text = "New status:", AutoSize = true, Margin = new Padding(8, 9, 2, 0) });
            cmbOrderStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            cmbOrderStatus.Items.AddRange(new object[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" });
            cmbOrderStatus.SelectedIndex = 0;
            buttons.Controls.Add(cmbOrderStatus);
            buttons.Controls.Add(MakeButton("Update Status", (s, e) => UpdateSelectedOrderStatus()));
            buttons.Controls.Add(MakeButton("Shipping Manifest", (s, e) => ShowManifest()));
            buttons.Controls.Add(MakeButton("Refresh", (s, e) => RefreshOrders()));
            page.Controls.Add(dgvOrders);
            page.Controls.Add(buttons);
            return page;
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
        }

        private Button MakeButton(string text, EventHandler handler)
        {
            Button button = new Button { Text = text, AutoSize = true, Height = 30, Margin = new Padding(4) };
            button.Click += handler;
            return button;
        }

        private void RefreshItems()
        {
            try { dgvItems.DataSource = db.GetItems(); }
            catch (Exception ex) { ShowError("Could not load Items", ex); }
        }

        private void RefreshAgents()
        {
            try { dgvAgents.DataSource = db.GetAgents(); }
            catch (Exception ex) { ShowError("Could not load Agents", ex); }
        }

        private void RefreshOrders()
        {
            try { dgvOrders.DataSource = db.GetOrders(); }
            catch (Exception ex) { ShowError("Could not load Orders", ex); }
        }

        private int SelectedId(DataGridView grid, string column)
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells[column].Value == null)
                throw new InvalidOperationException("Select a record first.");
            return Convert.ToInt32(grid.CurrentRow.Cells[column].Value);
        }

        private void AddItem()
        {
            string[] values = EditDialog.Show(this, "Add Item",
                new string[] { "Item name", "Size", "Price", "Stock quantity" },
                new string[] { "", "", "0", "0" });
            if (values == null) return;
            try
            {
                decimal price; int stock;
                if (String.IsNullOrWhiteSpace(values[0]) || !Decimal.TryParse(values[2], out price) ||
                    price < 0 || !Int32.TryParse(values[3], out stock) || stock < 0)
                    throw new ArgumentException("Enter a name, a valid non-negative price, and a non-negative stock quantity.");
                db.AddItem(values[0].Trim(), values[1].Trim(), price, stock);
                RefreshItems();
            }
            catch (Exception ex) { ShowError("Could not add Item", ex); }
        }

        private void EditItem()
        {
            try
            {
                int id = SelectedId(dgvItems, "ItemID");
                DataGridViewRow row = dgvItems.CurrentRow;
                string[] values = EditDialog.Show(this, "Edit Item",
                    new string[] { "Item name", "Size", "Price", "Stock quantity" },
                    new string[] { Convert.ToString(row.Cells["ItemName"].Value),
                        Convert.ToString(row.Cells["Size"].Value),
                        Convert.ToString(row.Cells["Price"].Value),
                        Convert.ToString(row.Cells["StockQuantity"].Value) });
                if (values == null) return;
                decimal price; int stock;
                if (String.IsNullOrWhiteSpace(values[0]) || !Decimal.TryParse(values[2], out price) ||
                    price < 0 || !Int32.TryParse(values[3], out stock) || stock < 0)
                    throw new ArgumentException("Enter a name, a valid non-negative price, and a non-negative stock quantity.");
                db.UpdateItem(id, values[0].Trim(), values[1].Trim(), price, stock);
                RefreshItems();
            }
            catch (Exception ex) { ShowError("Could not edit Item", ex); }
        }

        private void DeleteItem()
        {
            try
            {
                int id = SelectedId(dgvItems, "ItemID");
                if (MessageBox.Show("Delete the selected Item?", "Confirm delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                db.DeleteItem(id);
                RefreshItems();
            }
            catch (Exception ex) { ShowError("Could not delete Item. It may already be used in an order.", ex); }
        }

        private void AddAgent()
        {
            string[] values = EditDialog.Show(this, "Add Agent",
                new string[] { "Agent name", "Address", "Phone" }, new string[] { "", "", "" });
            if (values == null) return;
            try
            {
                if (String.IsNullOrWhiteSpace(values[0])) throw new ArgumentException("Agent name is required.");
                db.AddAgent(values[0].Trim(), values[1].Trim(), values[2].Trim());
                RefreshAgents();
            }
            catch (Exception ex) { ShowError("Could not add Agent", ex); }
        }

        private void EditAgent()
        {
            try
            {
                int id = SelectedId(dgvAgents, "AgentID");
                DataGridViewRow row = dgvAgents.CurrentRow;
                string[] values = EditDialog.Show(this, "Edit Agent",
                    new string[] { "Agent name", "Address", "Phone" },
                    new string[] { Convert.ToString(row.Cells["AgentName"].Value),
                        Convert.ToString(row.Cells["Address"].Value),
                        Convert.ToString(row.Cells["Phone"].Value) });
                if (values == null) return;
                if (String.IsNullOrWhiteSpace(values[0])) throw new ArgumentException("Agent name is required.");
                db.UpdateAgent(id, values[0].Trim(), values[1].Trim(), values[2].Trim());
                RefreshAgents();
            }
            catch (Exception ex) { ShowError("Could not edit Agent", ex); }
        }

        private void DeleteAgent()
        {
            try
            {
                int id = SelectedId(dgvAgents, "AgentID");
                if (MessageBox.Show("Delete the selected Agent?", "Confirm delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                db.DeleteAgent(id);
                RefreshAgents();
            }
            catch (Exception ex) { ShowError("Could not delete Agent. The Agent may be linked to existing orders or users.", ex); }
        }

        private void UpdateSelectedOrderStatus()
        {
            try
            {
                int id = SelectedId(dgvOrders, "OrderID");
                if (cmbOrderStatus.SelectedItem == null) return;
                db.UpdateOrderStatus(id, Convert.ToString(cmbOrderStatus.SelectedItem));
                RefreshOrders();
                MessageBox.Show("Order status updated.");
            }
            catch (Exception ex) { ShowError("Could not update order status", ex); }
        }

        private void ShowManifest()
        {
            try
            {
                int orderId = SelectedId(dgvOrders, "OrderID");
                manifestData = db.GetOrderManifest(orderId);
                if (manifestData.Rows.Count == 0)
                {
                    MessageBox.Show("No order details were found for this order.");
                    return;
                }
                manifestLineIndex = 0;
                using (PrintPreviewDialog preview = new PrintPreviewDialog())
                {
                    preview.Document = manifestDocument;
                    preview.Width = 1000;
                    preview.Height = 700;
                    preview.ShowDialog(this);
                }
                DialogResult print = MessageBox.Show(
                    "To save this manifest as a PDF, choose Yes and select 'Microsoft Print to PDF'.\nChoose No to close.",
                    "Print or save manifest", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (print == DialogResult.Yes)
                {
                    using (PrintDialog dialog = new PrintDialog())
                    {
                        dialog.Document = manifestDocument;
                        dialog.AllowSomePages = false;
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            manifestLineIndex = 0;
                            manifestDocument.Print();
                        }
                    }
                }
            }
            catch (Exception ex) { ShowError("Could not create Shipping Manifest", ex); }
        }

        private void ManifestPrintPage(object sender, PrintPageEventArgs e)
        {
            if (manifestData == null || manifestData.Rows.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }

            Graphics g = e.Graphics;
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            using (Font title = new Font("Arial", 18, FontStyle.Bold))
            using (Font bold = new Font("Arial", 10, FontStyle.Bold))
            using (Font normal = new Font("Arial", 10))
            using (Pen line = new Pen(Color.Black))
            {
                DataRow first = manifestData.Rows[0];
                g.DrawString("B2B WHOLESALE - SHIPPING MANIFEST", title, Brushes.Black, x, y);
                y += 42;
                g.DrawString("Order ID: " + first["OrderID"], bold, Brushes.Black, x, y); y += 20;
                g.DrawString("Order Date: " + Convert.ToDateTime(first["OrderDate"]).ToString("dd MMM yyyy"),
                    normal, Brushes.Black, x, y); y += 20;
                g.DrawString("Status: " + first["OrderStatus"], normal, Brushes.Black, x, y); y += 25;
                g.DrawString("Agent: " + first["AgentName"], bold, Brushes.Black, x, y); y += 20;
                g.DrawString("Address: " + first["Address"], normal, Brushes.Black, x, y); y += 20;
                g.DrawString("Phone: " + first["Phone"], normal, Brushes.Black, x, y); y += 30;

                g.DrawLine(line, x, y, e.MarginBounds.Right, y); y += 8;
                g.DrawString("Item", bold, Brushes.Black, x, y);
                g.DrawString("Size", bold, Brushes.Black, x + 260, y);
                g.DrawString("Qty", bold, Brushes.Black, x + 350, y);
                g.DrawString("Unit Amount", bold, Brushes.Black, x + 410, y);
                g.DrawString("Line Total", bold, Brushes.Black, x + 520, y);
                y += 22;
                g.DrawLine(line, x, y, e.MarginBounds.Right, y); y += 8;

                decimal total = 0;
                while (manifestLineIndex < manifestData.Rows.Count && y < e.MarginBounds.Bottom - 55)
                {
                    DataRow row = manifestData.Rows[manifestLineIndex];
                    decimal lineTotal = Convert.ToDecimal(row["LineTotal"]);
                    total += lineTotal;
                    g.DrawString(Convert.ToString(row["ItemName"]), normal, Brushes.Black, x, y);
                    g.DrawString(Convert.ToString(row["Size"]), normal, Brushes.Black, x + 260, y);
                    g.DrawString(Convert.ToString(row["Quantity"]), normal, Brushes.Black, x + 350, y);
                    g.DrawString(Convert.ToDecimal(row["UnitAmount"]).ToString("N2"), normal, Brushes.Black, x + 410, y);
                    g.DrawString(lineTotal.ToString("N2"), normal, Brushes.Black, x + 520, y);
                    y += 22;
                    manifestLineIndex++;
                }

                if (manifestLineIndex < manifestData.Rows.Count)
                {
                    e.HasMorePages = true;
                    return;
                }

                g.DrawLine(line, x, y + 5, e.MarginBounds.Right, y + 5);
                g.DrawString("Total: " + Convert.ToDecimal(
                    manifestData.Compute("SUM(LineTotal)", "")).ToString("N2"),
                    bold, Brushes.Black, x + 420, y + 15);
                g.DrawString("Prepared by: " + adminUser, normal, Brushes.Black, x, y + 45);
            }
            e.HasMorePages = false;
        }

        private void ShowError(string title, Exception ex)
        {
            MessageBox.Show(title + ":\n" + ex.Message, "B2B Wholesale",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal class EditDialog : Form
    {
        private readonly TextBox[] inputs;
        private EditDialog(string title, string[] labels, string[] values)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 430;
            Height = 145 + labels.Length * 42;
            inputs = new TextBox[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                Controls.Add(new Label { Text = labels[i], AutoSize = true,
                    Location = new Point(18, 22 + i * 42) });
                inputs[i] = new TextBox { Location = new Point(145, 17 + i * 42), Width = 245,
                    Text = values[i] ?? "" };
                Controls.Add(inputs[i]);
            }

            Button ok = new Button { Text = "Save", DialogResult = DialogResult.OK,
                Location = new Point(145, 28 + labels.Length * 42), Width = 90 };
            Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(245, 28 + labels.Length * 42), Width = 90 };
            Controls.Add(ok); Controls.Add(cancel);
            AcceptButton = ok; CancelButton = cancel;
        }

        public static string[] Show(IWin32Window owner, string title, string[] labels, string[] values)
        {
            using (EditDialog dialog = new EditDialog(title, labels, values))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK) return null;
                string[] result = new string[dialog.inputs.Length];
                for (int i = 0; i < result.Length; i++) result[i] = dialog.inputs[i].Text;
                return result;
            }
        }
    }
}
