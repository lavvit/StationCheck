using MathNet.Numerics.Distributions;
using SeaDropRich;

namespace StationCheck
{
    public class Visuable : Scene
    {
        public static double Average = 100, Mu = 0, Sigma = 1, Offset = 0;

        //初期化
        //public static void Main(string[] args)
        //{
        //    DXLib.Init(new Program(), 1280, 720);
        //}


        //描画
        public override void Draw()
        {
            //X軸
            for (int i = 0; i < 11; i++)
            {
                Drawing.Line(135 + 100 * i, 600, 0, -600, 0x606060, 1);
                Drawing.Text(135 + 100 * i, 600, i);
            }
            //Y軸
            for (int i = 0; i < 40; i++)
            {
                bool isfive = i % 5 == 0;
                double value = Average * i / 10;
                Drawing.Line(135, 600 - 20 * i, 1000, 0, isfive ? 0xcccccc : 0x606060, isfive ? 2 : 1);
                if (i % 10 == 0) Drawing.Text(100, 600 - 8 - 20 * i, value, i == 10 ? 0xffff00 : 0xffffff);
            }

            (int x, double val) max = (0, 0);
            for (int i = 0; i < 1000; i++)
            {
                double value = Average * Value(Mu, Sigma, i / 100.0 - Offset);
                Drawing.Box(135 + i, 601, 1, -value * 2, 0x00ffff);
                if (value > max.val) max = (i, value);
            }
            Drawing.Text(135 + max.x, 600 - 16 - max.val, $"{max.x / 100.0} , {max.val}");

            Drawing.Text(20, 40, $"Mu : {Mu}");
            Drawing.Text(20, 60, $"Sigma : {Sigma}");
            Drawing.Text(20, 80, $"Offset : {Offset}");
            base.Draw();
        }

        //処理
        public override void Update()
        {
            //Sigma の上下
            if (!Key.IsPushing(EKey.LShift))
            {
                if (Key.IsPushed(EKey.Right)) Sigma += 0.1;
                if (Key.IsPushed(EKey.Left)) Sigma -= 0.1;
                if (Key.IsPushed(EKey.Enter)) Sigma = 1;
                if (Sigma < 0) Sigma = 0;
                Sigma = Math.Round(Sigma, 3);
            }

            //Mu の上下
            {
                if (Key.IsPushed(EKey.Up)) Mu += 0.1;
                if (Key.IsPushed(EKey.Down)) Mu -= 0.1;
                if (Key.IsPushed(EKey.Enter)) Mu = 1;
                Mu = Math.Round(Mu, 3);
            }

            //Offset の上下
            if (Key.IsPushing(EKey.LShift))
            {
                if (Key.IsPushed(EKey.Right)) Offset += 0.1;
                if (Key.IsPushed(EKey.Left)) Offset -= 0.1;
            }

            //終了
            if (Key.IsPushed(EKey.Esc)) DXLib.End();
            base.Update();
        }

        /// <summary>
        /// 対数正規分布を使用してX軸ごとのY軸を求める。
        /// </summary>
        /// <param name="mu">平均</param>
        /// <param name="sigma">標準偏差</param>
        /// <param name="x">X軸の値</param>
        /// <returns></returns>
        public static double Value(double mu, double sigma, double x)
        {
            return LogNormal.PDF(mu, sigma, x);
        }
    }
}
