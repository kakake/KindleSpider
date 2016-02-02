using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KindleSpider.BookSP;
using KindleSpider;
using System.Threading;

namespace TestKindle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        private void btnSearch_Click(object sender, EventArgs e)
        {
            List<ResultBook> booklist = SearchBook.Search(txtKeyChar.Text);
            gridbook.DataSource = booklist;
        }

        private string geturl()
        {
            string url = "";
            if (radioButton1.Checked)
                url = txtUrl.Text;
            else
            {
                if (gridbook.CurrentCell != null)
                {
                    List<ResultBook> booklist = gridbook.DataSource as List<ResultBook>;
                    url = booklist[gridbook.CurrentCell.RowIndex].bookurl;
                }
            }
            return url;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string url = geturl();
                if (url != "")
                {
                    Book book = BookParse.ParseSingleBook(url);
                    book.SaveTxt("booktxt/" + book.title + ".txt");
                    MessageBox.Show("操作成功！");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void btnpdf_Click(object sender, EventArgs e)
        {
            try
            {
                string url = geturl();
                if (url != "")
                {
                    Book book = BookParse.ParseSingleBook(url);
                    book.SavePdf("bookpdf/" + book.title + ".pdf");
                    MessageBox.Show("操作成功！");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void btnsendtxt_Click(object sender, EventArgs e)
        {
            try
            {
                string url = geturl();
                if (url != "")
                {
                    Book book = BookParse.ParseSingleBook(url);
                    book.SendMail(txtemail.Text, 0, "booktxt/" + book.title + ".txt");
                    MessageBox.Show("操作成功！");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void btnsendpdf_Click(object sender, EventArgs e)
        {
            try
            {
                string url = geturl();
                if (url != "" && txtemail.Text.Trim() != "")
                {
                    Book book = BookParse.ParseSingleBook(url);
                    book.SendMail(txtemail.Text, 1, "bookpdf/" + book.title + ".pdf");
                    MessageBox.Show("操作成功！");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(geturl());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                Book book = new Book();
                book.SendMail(txtemail.Text, 2, filename);
                MessageBox.Show("操作成功！");
            }
        }

        private void txtemail_DoubleClick(object sender, EventArgs e)
        {
            txtemail.Text = "@kindle.cn";
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            ThreadStart starter = new ThreadStart(this.SpiderThread);
            Thread spider = new Thread(starter);
            spider.Start();
        }

        public void SpiderThread()
        {
            BookParse.ParseWebsite(txtWebSite.Text, null, null);
        }

        private void report(string currentUrl, string elapsed, string processedUrlCount)
        {
            textBox1.Text = currentUrl;
            textBox2.Text = elapsed;
            textBox3.Text = processedUrlCount;
        }
    }
}
