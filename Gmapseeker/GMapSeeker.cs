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
using System.IO;

namespace Gmapseeker
{
    public partial class GMapSeeker : Form
    {
        private List<Shop> shops;
        private string search;
        private string city;
        private string country;
        private double rating;
        private string nextPageToken = string.Empty;
        private List<string> emailList = new List<string>();

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
            emailsPanel.Visible = false;
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

            emailsCollector();
        }

        private void emailsCollector()
        {
            
            foreach (var shop in shops)
            {
                if (!string.IsNullOrEmpty(shop.email) && shop.email != "None")
                {
                    emailList.Add(shop.email);
                }
            }

            if (emailList.Count == 0)
            {
                Console.WriteLine("No emails captured!");
            }
            else
            {
                listEmails.Clear();
                listEmails.Text = string.Join(Environment.NewLine, emailList);
                emailsPanel.Visible = true;
            }
        }

        private void SaveEmailListToFile()
        {
            // Create a SaveFileDialog instance
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set the initial directory and default file name
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.FileName = "email_list.txt";

            // Set the file filter to restrict file types
            saveFileDialog.Filter = "Text Files|*.txt|All Files|*.*";

            // Show the dialog and get the result
            DialogResult result = saveFileDialog.ShowDialog();

            // Check if the user clicked the Save button
            if (result == DialogResult.OK)
            {
                // Get the selected file path from the dialog
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Write the email list content to the selected file
                    File.WriteAllText(filePath, listEmails.Text);
                    Console.WriteLine("Email list saved successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred while saving the email list: " + ex.Message);
                }
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // Clear existing controls from the flow layout panel
            flowLayoutPanel.Controls.Clear();
            listEmails.Clear();
            emailList.Clear();

            // Validate input fields
            if (string.IsNullOrEmpty(txtIndustry.Text) || string.IsNullOrEmpty(txtCity.Text) || cmbCountry.SelectedItem == null || cmbRating.SelectedItem == null)
            {
                MessageBox.Show("Please enter values for Industry, City, Country and select Rating.", "Validation Error",
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
            cmbRating.Enabled = false;

            search = txtIndustry.Text;
            city = txtCity.Text;
            country = cmbCountry.SelectedItem?.ToString();
            rating = Convert.ToDouble(cmbRating.SelectedItem?.ToString());


            // Retrieve shop data from Google Maps API
            GoogleMapsAPI api = new GoogleMapsAPI();
            (shops, nextPageToken) = await Task.Run(() => api.GetShops(search, city, country));

            // Filter shops based on minimum rating
            shops = shops.Where(shop => shop.rating >= rating).ToList();


            //Creating items
            populateItems(shops);

            progressBar.Visible = false;
            progressBar.Enabled = false;
            btnSearch.Visible = true;
            btnSearch.Enabled = true;
            txtIndustry.Enabled = true;
            txtCity.Enabled = true;
            cmbCountry.Enabled = true;
            cmbRating.Enabled = true;
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
            cmbRating.Enabled = false;
            btnNext.Enabled = false;
            btnNext.Visible = false;

            // Retrieve the next page of results
            GoogleMapsAPI api = new GoogleMapsAPI();
            var result = await Task.Run(() => api.FetchNextPage(nextPageToken));
            shops = result.shops;
            nextPageToken = result.nextPageToken;

            // Filter shops based on minimum rating
            shops = shops.Where(shop => shop.rating >= rating).ToList();

            //Creating items for the next page
            populateItems(shops);

            progressBar.Visible = false;
            progressBar.Enabled = false;
            btnSearch.Visible = true;
            btnSearch.Enabled = true;
            txtIndustry.Enabled = true;
            txtCity.Enabled = true;
            cmbCountry.Enabled = true;
            cmbRating.Enabled = true;
            btnNext.Enabled = true;
            btnNext.Visible = true;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveEmailListToFile();
        }

        private async void btnAuto_Click(object sender, EventArgs e)
        {
            // Clear existing controls from the flow layout panel
            flowLayoutPanel.Controls.Clear();
            listEmails.Clear();
            emailList.Clear();

            // Validate input fields
            if (string.IsNullOrEmpty(txtIndustry.Text) || string.IsNullOrEmpty(txtCity.Text) || cmbCountry.SelectedItem == null || cmbRating.SelectedItem == null)
            {
                MessageBox.Show("Please enter values for Industry, City, Country and select Rating.", "Validation Error",
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
            cmbRating.Enabled = false;
            btnNext.Enabled = false;
            btnNext.Visible = false;

            search = txtIndustry.Text;
            city = txtCity.Text;
            country = cmbCountry.SelectedItem?.ToString();
            rating = Convert.ToDouble(cmbRating.SelectedItem?.ToString());

            // Retrieve shop data from Google Maps API
            GoogleMapsAPI api = new GoogleMapsAPI();
            (shops, nextPageToken) = await Task.Run(() => api.GetShops(search, city, country));

            while (!string.IsNullOrEmpty(nextPageToken))
            {
                // Retrieve the next page of results
                var result = await Task.Run(() => api.FetchNextPage(nextPageToken));
                shops.AddRange(result.shops);
                nextPageToken = result.nextPageToken;
            }

            // Filter shops based on minimum rating
            shops = shops.Where(shop => shop.rating >= rating).ToList();

            //Creating items for the next page
            populateItems(shops);

            progressBar.Visible = false;
            progressBar.Enabled = false;
            btnSearch.Visible = true;
            btnSearch.Enabled = true;
            txtIndustry.Enabled = true;
            txtCity.Enabled = true;
            cmbCountry.Enabled = true;
            cmbRating.Enabled = true;
            btnNext.Enabled = false;
            btnNext.Visible = true;
        }
    }
}
