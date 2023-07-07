using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Helpers;

namespace Gmapseeker
{
    public partial class GMapSeeker : Form
    {
        private List<Shop> shops;
        private string search;
        private string city;
        private string country;
        private string nextPageToken = string.Empty;

        public GMapSeeker()
        {
            InitializeComponent();
        }

        private void GMapSeeker_Load(object sender, EventArgs e)
        {
            flowLayoutPanel.Controls.Clear();
            progressBar.Enabled = false;
            progressBar.Visible = false;
            btnNext.Enabled = false;
            btnNext.Visible = false;
        }

        private void populateItems(List<Shop> shops)
        {

            // Clear existing controls from the flow layout panel
            flowLayoutPanel.Controls.Clear();

            // Loop through each shop and create ListItem controls
            foreach (var shop in shops)
            {
                ListItem listItem = new ListItem();
                listItem.TheName = shop.name;
                listItem.Rating = shop.rating;
                listItem.Address = shop.address;
                listItem.Mobile = shop.international_phone;
                listItem.Website = shop.website;
                listItem.Email = shop.email;

                // Add ListItem to the flow layout panel
                flowLayoutPanel.Controls.Add(listItem);
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // Clear existing controls from the flow layout panel
            flowLayoutPanel.Controls.Clear();

            // Validate input fields
            if (string.IsNullOrEmpty(txtIndustry.Text) || string.IsNullOrEmpty(txtCity.Text) || cmbCountry.SelectedItem == null)
            {
                MessageBox.Show("Validation Error", "Please enter values for Industry, City, and select a Country.",
            MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            btnSearch.Enabled = false;
            btnSearch.Visible = false;
            progressBar.Visible = true;
            progressBar.Enabled = true;
            txtIndustry.Enabled = false;
            txtCity.Enabled = false;
            cmbCountry.Enabled = false;

            search = txtIndustry.Text;
            city = txtCity.Text;
            country = cmbCountry.SelectedText.ToString();


            // Retrieve shop data from Google Maps API
            GoogleMapsAPI api = new GoogleMapsAPI();
            (shops, nextPageToken) = await Task.Run(() => api.GetShops(search, city, country));


            //Creating items
            populateItems(shops);

            progressBar.Visible = false;
            progressBar.Enabled = false;
            btnSearch.Visible = true;
            btnSearch.Enabled = true;
            txtIndustry.Enabled = true;
            txtCity.Enabled = true;
            cmbCountry.Enabled = true;
            btnNext.Enabled = true;
            btnNext.Visible = true;
        }

        private async void btnNext_ClickAsync(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nextPageToken))
            {
                // No more pages available
                MessageBox.Show("No more results available.", "End of Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Clear existing controls from the flow layout panel
            flowLayoutPanel.Controls.Clear();

            btnSearch.Enabled = false;
            btnSearch.Visible = false;
            progressBar.Visible = true;
            progressBar.Enabled = true;
            txtIndustry.Enabled = false;
            txtCity.Enabled = false;
            cmbCountry.Enabled = false;
            btnNext.Enabled = false;
            btnNext.Visible = false;

            // Retrieve the next page of results
            GoogleMapsAPI api = new GoogleMapsAPI();
            var result = await Task.Run(() => api.FetchNextPage(nextPageToken));
            shops = result.shops;
            nextPageToken = result.nextPageToken;

            //Creating items for the next page
            populateItems(shops);

            progressBar.Visible = false;
            progressBar.Enabled = false;
            btnSearch.Visible = true;
            btnSearch.Enabled = true;
            txtIndustry.Enabled = true;
            txtCity.Enabled = true;
            cmbCountry.Enabled = true;
            btnNext.Enabled = true;
            btnNext.Visible = true;

        }
    }
}
