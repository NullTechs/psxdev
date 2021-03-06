﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace RubberTool
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        private int kpIndexLeft = 1;
        private int kpIndexRight = 1;

        public Form1()
        {
            InitializeComponent();

#if DEBUG
            AllocConsole();
#endif

            Console.WriteLine("RubberTool 1.0");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout about = new FormAbout();
            about.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loadLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if ( result == DialogResult.OK)
            {
                entityBox1.Image0 = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void loadRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                entityBox2.Image0 = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            entityBox1.Mode = EntityMode.ViasConnect;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            entityBox1.Mode = EntityMode.Selection;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            entityBox2.Mode = EntityMode.ViasConnect;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            entityBox2.Mode = EntityMode.Selection;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            entityBox1.DrawRegionBetweenSelectedViases();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            entityBox2.DrawRegionBetweenSelectedViases();
        }

        private void clearLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            entityBox1.Image0 = null;
        }

        private void clearRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            entityBox2.Image0 = null;
        }

        private void saveLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                entityBox1.Image0.Save(saveFileDialog1.FileName);
            }
        }

        private void saveRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                entityBox2.Image0.Save(saveFileDialog1.FileName);
            }
        }

        #region Названия контрольных точек

        private void entityBox1_OnEntitySelect(object sender, Entity entity, EventArgs e)
        {
            Entity last = entityBox1.GetLastSelected();

            toolStripTextBox1.Text = last.Label;
        }

        private void entityBox2_OnEntitySelect(object sender, Entity entity, EventArgs e)
        {
            Entity last = entityBox2.GetLastSelected();

            toolStripTextBox2.Text = last.Label;
        }

        private void toolStripTextBox1_Leave(object sender, EventArgs e)
        {
            Entity last = entityBox1.GetLastSelected();

            last.Label = toolStripTextBox1.Text;
        }

        private void toolStripTextBox2_Leave(object sender, EventArgs e)
        {
            Entity last = entityBox2.GetLastSelected();

            last.Label = toolStripTextBox2.Text;
        }

        #endregion

        private void entityBox1_OnEntityAdd(object sender, Entity entity, EventArgs e)
        {
            if ( entityBox1.IsEntityVias(entity))
            {
                entity.Label = "kp" + kpIndexLeft.ToString();
                kpIndexLeft++;
            }
        }

        private void entityBox2_OnEntityAdd(object sender, Entity entity, EventArgs e)
        {
            if (entityBox2.IsEntityVias(entity))
            {
                entity.Label = "kp" + kpIndexRight.ToString();
                kpIndexRight++;
            }
        }

        /// <summary>
        /// Из левой части с изображением и тремя точками нарисовать треугольник в правой части
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawTexturedTriangleFromLeftImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Entity> _selectedLeft = entityBox1.GetSelected();
            List<Entity> _selectedRight = entityBox2.GetSelected();

            //
            // Проверить что изображение в левой части загружено
            //

            if ( entityBox1.Image0 == null )
            {
                MessageBox.Show("Load image on Left");
                return;
            }

            //
            // Проверить что ключевые точки слева выделены правильно
            //

            if (_selectedLeft.Count != 3 )
            {
                MessageBox.Show("Select exactly 3 keypoints to form triangle");
                return;
            }

            foreach ( Entity entity in _selectedLeft)
            {
                if ( !entityBox1.IsEntityVias(entity))
                {
                    MessageBox.Show("Select keypoints only");
                    return;
                }
            }

            //
            // Проверить что ключевые точки справа выделены правильно
            //

            if (_selectedRight.Count != 3)
            {
                MessageBox.Show("Select exactly 3 keypoints to form triangle");
                return;
            }

            foreach (Entity entity in _selectedRight)
            {
                if (!entityBox1.IsEntityVias(entity))
                {
                    MessageBox.Show("Select keypoints only");
                    return;
                }
            }

            //
            // Сформировать изображение справа
            //

            Bitmap bitmap = new Bitmap ( entityBox1.Image0.Width * 3, entityBox1.Image0.Height * 3);
            Graphics gr = Graphics.FromImage(bitmap);
            gr.Clear(Color.White);
            entityBox2.Image0 = bitmap;

            //
            // Сформировать параметры
            //

            Triangle sourceTri = new Triangle();

            Point a0 = entityBox1.LambdaToScreen(_selectedLeft[0].LambdaX, _selectedLeft[0].LambdaY);
            Point a1 = entityBox1.LambdaToScreen(_selectedLeft[1].LambdaX, _selectedLeft[1].LambdaY);
            Point a2 = entityBox1.LambdaToScreen(_selectedLeft[2].LambdaX, _selectedLeft[2].LambdaY);

            sourceTri.a = new Point2D(a0.X, a0.Y);
            sourceTri.b = new Point2D(a1.X, a1.Y);
            sourceTri.c = new Point2D(a2.X, a2.Y);

            Triangle destTri = new Triangle();

            Point b0 = entityBox1.LambdaToScreen(_selectedRight[0].LambdaX, _selectedRight[0].LambdaY);
            Point b1 = entityBox1.LambdaToScreen(_selectedRight[1].LambdaX, _selectedRight[1].LambdaY);
            Point b2 = entityBox1.LambdaToScreen(_selectedRight[2].LambdaX, _selectedRight[2].LambdaY);

            destTri.a = new Point2D(b0.X, b0.Y);
            destTri.b = new Point2D(b1.X, b1.Y);
            destTri.c = new Point2D(b2.X, b2.Y);

            NonAffineTransform.WarpTriangle(entityBox1.Image0,
                entityBox2.Image0,
                sourceTri, destTri);

        }

        private void lineCrossTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Vec v1 = new Vec(new Point2D(75F, 53F), new Point2D(96F,80F));
            Vec v2 = new Vec(new Point2D(250F, 50F), new Point2D(214F, 74F));

            Point2D cross = VecMath.LineCross(v1, v2);

            Console.WriteLine( "X: " + cross.X.ToString() + ", Y:" + cross.Y.ToString() );
        }

        private void barycenterTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Triangle tri = new Triangle();

            tri.a = new Point2D(751, 294);
            tri.b = new Point2D(526, 546);
            tri.c = new Point2D(866, 626);

            Point2D c = VecMath.BaryCenter(tri);

            Console.WriteLine("X: " + c.X.ToString() + ", Y: " + c.Y.ToString());
        }

        private void trilateralTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Triangle sourceTri = new Triangle();

            sourceTri.a = new Point2D(466, 191);
            sourceTri.b = new Point2D(285, 419);
            sourceTri.c = new Point2D(636, 462);

            Triangle destTri = new Triangle();

            destTri.a = new Point2D(316, 243);
            destTri.b = new Point2D(194, 350);
            destTri.c = new Point2D(383, 372);

            Point2D n = new Point2D(268, 285);

            Point2D n2 = NonAffineTransform.TrilateralXform(sourceTri, destTri, n);

            Console.WriteLine("X: " + n2.X.ToString() + ", Y: " + n2.Y.ToString());

        }
    }
}
