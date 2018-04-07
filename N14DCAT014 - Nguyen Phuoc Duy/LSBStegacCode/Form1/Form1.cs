using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Form1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Oepn File Button
        public void buttonOpen_Click(object sender, EventArgs e)
        {
            //Mở File
            OpenFileDialog openDialog = new OpenFileDialog();
            //Lọc File
            openDialog.Filter = "Image Files (*.png) | *.png";
            //Default Path
            openDialog.InitialDirectory = @"C:\";
            //Ghi File Path vào Textbox
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = openDialog.FileName.ToString();
                pictureBox1.ImageLocation = textBoxFilePath.Text;
            }
        }
        //padding bit 0 nếu không đủ 8 bit
        public string padding(int b)
        {
            //Đổi sang hệ Bin dạng String
            string bin = Convert.ToString(b, 2);
            //Console.WriteLine("bin before: " + bin);
            while (bin.Length < 8)
            {
                bin = "0" + bin;
                //Console.WriteLine("bining: " + bin);
            }
            //Console.WriteLine("bin after: " + bin);
            return bin;
        }
        //Chuyển Chuỗi thành Binary
        public string MessageToBinary(string msg)
        {
            string result = "";

            for (int i = 0; i < msg.ToCharArray().Length; i++)
            {
                char c = msg.ToCharArray()[i];
                result += padding(c);
            }
            return result;
        }
        //Chuyển chuỗi Binary thành Byte - để kết hợp với Hàm Encode,ASCII
        public static Byte[] GetBytesFromBinaryString(string binary)
        {
            Console.WriteLine("byte" + binary);
            var list = new List<Byte>();

            for (int i = 0; i < binary.Length-1; i += 8)
            {
                string t = binary.Substring(i, 8);

                list.Add(Convert.ToByte(t, 2));
            }

            return list.ToArray();
        }

        //Button Encode
        public void buttonEncode_Click(object sender, EventArgs e)
        {
            int count = 0;
            //Chuyển Message thành Bin dạng String
            string msgInBin = MessageToBinary(textBoxMessage.Text);
            //Thêm độ dài message (Hệ bin) vào đầu Chuỗi Bin
            string hiddenString = padding(textBoxMessage.Text.Length) + msgInBin;
            //Thêm vài số 0 đăng sau để độ dài chuỗi chia hết cho 3 vì Pixel có 3 giá trị R G B
            while (hiddenString.Length % 3 != 0)
            {
                hiddenString += "0";
            }
            //Chuyển thành Mảng Char
            char[] binHidden = hiddenString.ToCharArray();
            //Console.WriteLine(binHidden);
            //Lấy Ảnh
            Bitmap img = new Bitmap(textBoxFilePath.Text);
            //Lấy từng Pixel
            for (int row = 0; row < img.Width; row++)
            {
                for (int column = 0; column < img.Height; column++)
                {
                    if (count == binHidden.Length)
                        break;
                    Color pixel = img.GetPixel(row, column);
                    //Console.WriteLine("old pixel: " + pixel);
                    int newPixelR = (binHidden[count++] == '0') ? (pixel.R & 254) : (pixel.R | 1);
                    int newPixelG = (binHidden[count++] == '0') ? (pixel.G & 254) : (pixel.G | 1);
                    int newPixelB = (binHidden[count++] == '0') ? (pixel.B & 254) : (pixel.B | 1);
                    img.SetPixel(row, column, Color.FromArgb(newPixelR, newPixelG, newPixelB));
                    //Console.WriteLine("Color: " + newPixelR + " " + newPixelG + " " + newPixelB);
                    //Console.WriteLine("new pixel: " + img.GetPixel(row, column));
                }
                if (count == binHidden.Length)
                    break;
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Image Files (*.png) | *.png";
            saveFile.InitialDirectory = @"C:\";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = saveFile.FileName.ToString();
                pictureBox1.ImageLocation = textBoxFilePath.Text;

                img.Save(textBoxFilePath.Text);
                textBoxMessage.Clear();         //Xóa Message
            }
        }

        //Lấy LSB trong Pixel
        public static string getLSBBits(Color pixel)
        {
            string result = "";
            string redInPixel = Convert.ToString(pixel.R, 2);
            string greenInPixel = Convert.ToString(pixel.G, 2);
            string blueInPixel = Convert.ToString(pixel.B, 2);
            //Console.WriteLine("Pixel " + redInPixel + " " + greenInPixel + " " + blueInPixel);
            result += redInPixel[redInPixel.Length - 1];
            result += greenInPixel[greenInPixel.Length - 1];
            result += blueInPixel[blueInPixel.Length - 1];

            return result;
        }
        //Button Decode
        public void buttonDecode_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(textBoxFilePath.Text);
            var message = "";
            int count = 0;
            int length = 0;

            for (int row = 0; row < img.Width; row++)
            {
                for (int column = 0; column < img.Height; column++)
                {
                    Color pixel = img.GetPixel(row, column);
                    //Console.WriteLine("Pixel get: " + pixel);
                    message += getLSBBits(pixel);
                    count += 3;        //Vì lấy 1 pixel thì ta sẽ được 3 LSB (R - G - B)
                    //Console.WriteLine("message" + message);
                    //Console.WriteLine("count" + count);
                    if (count == 9)
                    {
                        length = Convert.ToInt32(message.Substring(0, 8), 2); //Chuyển từ String sang Bin
                        //Console.WriteLine(length+1);
                        message = message.Substring(8); //Message sẽ từ vị trí thứ 8 trở đi (8 bit đầu là độ dài chuỗi)
                    }
                    if (count >= ((length+1) * 8)) //Length + 1 vì tính luôn 8 bit đầu của length
                    {
                        int du = count % ((length + 1) * 8);  //Phần dư, vì count + một lúc 3 đơn vị => sẽ có lúc vượt hơn length
                                                             // VD: 19 ký tự => length = 160 còn count = 162
                        message = message.Substring(0, message.Length - du);
                        break;
                    }

                }
                if (count >= ((length+1) * 8))
                        break;
            }
            var data2 = GetBytesFromBinaryString(message);
            message = Encoding.ASCII.GetString(data2);
            textBoxMessage.Text = message;
        }
    }
}
