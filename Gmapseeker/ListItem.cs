using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gmapseeker
{
    public partial class ListItem : UserControl
    {
        public ListItem()
        {
            InitializeComponent();
        }

        #region Properties

        private double _rating;
        private string _name;
        private string _address;
        private string _mobile;
        private string _website;
        private string _email;


        [Category("Custom Props")]
        public double Rating
        {
            get { return _rating; }
            set { _rating = value; lblRating.Text = value.ToString(); }
        }

        [Category("Custom Props")]
        public string TheName
        {
            get { return _name; }
            set { _name = value; lblName.Text = value; }
        }

        [Category("Custom Props")]
        public string Address
        {
            get { return _address; }
            set { _address = value; lblAddress.Text = value; }
        }

        [Category("Custom Props")]
        public string Mobile
        {
            get { return _mobile; }
            set { _mobile = value; lblMobile.Text = value; }
        }

        [Category("Custom Props")]
        public string Website
        {
            get { return _website; }
            set { _website = value; lblWebsite.Text = value; }
        }

        [Category("Custom Props")]
        public string Email
        {
            get { return _email; }
            set { _email = value; lblEmail.Text = value; }
        }


        #endregion

        private void ListItem_Load(object sender, EventArgs e)
        {

        }

        private void lblWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lblWebsite.Tag = lblWebsite.Text;
            string url = lblWebsite.Tag.ToString();

            // Open the URL in the default web browser
            System.Diagnostics.Process.Start(url);
        }
    }
}
