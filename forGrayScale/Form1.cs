using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace forGrayScale
{
    public partial class Form1 : Form
    {
        string dosyaYolu = string.Empty;
        Bitmap bmp = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGozat_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dosyaYolu = openFileDialog1.FileName;

                //bir bitmap nesnesi oluşturulur ve seçilen resim bu nesneye yüklenir.  

                bmp = new Bitmap(dosyaYolu);

                pictureBox1.Image = bmp;

                //picturebox nesnesinin sizemode özelliği strech olarak ayarlanır.Bunun //sebebi ise seçilen resmin picturebox nesnesinde tam olarak gözükmesini sağlamaktır.

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }

        }
        public Bitmap eightBppGrayScaleYap(Bitmap bmp)
        {
            int w = bmp.Width,
            h = bmp.Height,
            r, ic, oc, bmpStride, outputStride, bytesPerPixel;
            PixelFormat pfIn = bmp.PixelFormat;
            ColorPalette palette;
            Bitmap output;
            BitmapData bmpData, outputData;

            //Create the new bitmap
            output = new Bitmap(w, h, PixelFormat.Format8bppIndexed);

            //Build a grayscale color Palette
            palette = output.Palette;
            for (int i = 0; i < 256; i++)
            {
                Color tmp = Color.FromArgb(255, i, i, i);
                palette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            output.Palette = palette;

            //No need to convert formats if already in 8 bit
            if (pfIn == PixelFormat.Format8bppIndexed)
            {
                output = (Bitmap)bmp.Clone();

                //Make sure the palette is a grayscale palette and not some other
                //8-bit indexed palette
                output.Palette = palette;

                return output;
            }

            //Get the number of bytes per pixel
            switch (pfIn)
            {
                case PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                case PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                case PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                default: throw new InvalidOperationException("Image format not supported");
            }

            //Lock the images
            bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
            pfIn);
            outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
            PixelFormat.Format8bppIndexed);
            bmpStride = bmpData.Stride;
            outputStride = outputData.Stride;

            //Traverse each pixel of the image
            unsafe
            {
                byte* bmpPtr = (byte*)bmpData.Scan0.ToPointer(),
                outputPtr = (byte*)outputData.Scan0.ToPointer();

                if (bytesPerPixel == 3)
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = .299*R + .587*G + .114*B
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 3, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                            (0.299f * bmpPtr[r * bmpStride + ic] +
                            0.587f * bmpPtr[r * bmpStride + ic + 1] +
                            0.114f * bmpPtr[r * bmpStride + ic + 2]);
                }
                else //bytesPerPixel == 4
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = alpha * (.299*R + .587*G + .114*B)
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 4, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                            ((bmpPtr[r * bmpStride + ic] / 255.0f) *
                            (0.299f * bmpPtr[r * bmpStride + ic + 1] +
                            0.587f * bmpPtr[r * bmpStride + ic + 2] +
                            0.114f * bmpPtr[r * bmpStride + ic + 3]));
                }
            }

            //Unlock the images
            bmp.UnlockBits(bmpData);
            output.UnlockBits(outputData);

            return output;
        }
        private void btnGrayScale_Click(object sender, EventArgs e)
        {
            
             /* for (int y = 0; y < bmp.Height; y++) //Pixelleri boyuna olarak tarar.
            {
                for (int x = 0; x < bmp.Width; x++)//Pixelleri yatay olarak tarar.
                {
                    Color eski = bmp.GetPixel(x, y); //Sıradaki pixeli alır.
                    int ortalama = (eski.R + eski.G + eski.B) / 3; //ele alınan pixelin RGB kodlarının ortalamasını alır.
                    Color yeni = Color.FromArgb(eski.A, ortalama, ortalama, ortalama);//Bulunan ortalamanın RGB olarak renk değerini alır.
                    bmp.SetPixel(x, y, yeni);//Pixele yeni RGB kodlarını atar ve pixeli eski yerine koyar.
                }
            }*/
            
            pictureBox1.Image = eightBppGrayScaleYap(bmp);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Jpeg Resmi|*.jpg|Bitmap Resmi|*.bmp|Gif Resmi|*.gif";
            saveFileDialog1.Title = "Resmi Kaydet";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "") //Dosya adı boş değilse kaydedecek.
            {
                // FileStream nesnesi ile kayıtı gerçekleştirecek.
                FileStream DosyaAkisi = (FileStream)saveFileDialog1.OpenFile();
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox1.Image.Save(DosyaAkisi, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case 2:
                        pictureBox1.Image.Save(DosyaAkisi, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 3:
                        pictureBox1.Image.Save(DosyaAkisi, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
            }
            pictureBox1.Image = null;
        }
    }
}
