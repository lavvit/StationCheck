using MathNet.Numerics.Distributions;
using SeaDropRich;

namespace StationCheck
{
    public class Program : Scene
    {
        public static double Average = 50;
        public static List<(DateTime Time, int Value)> TimeData = new List<(DateTime, int)>();
        public static Dictionary<string, Type> TypeData = new Dictionary<string, Type>();
        public static List<(DateTime Time, string ID)> TrainData = new List<(DateTime, string)>();

        //初期化
        public static void Main(string[] args)
        {
            DXLib.Init(new Program(), 1280, 720);
        }


        //セット
        public override void Enable()
        {
            //時間別標準利用者数
            //対象となる時間及びベースとなる人数を入力する
            //例: 0700:30
            //時間:利用者数
            foreach (string line in Text.Read(@"Data\Time.txt"))
            {
                string[] split = line.Split(':');
                if (!int.TryParse(split[0], out int t)) continue;
                int.TryParse(split[1], out int value);
                DateTime time = new DateTime().AddHours(t / 100).AddMinutes(t % 100);
                TimeData.Add((time, value));
            }
            TimeData.Sort();

            //時間別直前利用者数

            //列車データ(時間及びMuやSigmaなど)を入力する
            //例: Test:試運転:1:1(:0xffffff)
            foreach (string line in Text.Read(@"Data\Type.txt"))
            {
                string[] split = line.Split(':');
                Type train = new Type();
                string id = split[0];
                train.Name = split[1];
                double.TryParse(split[2], out train.Mu);
                double.TryParse(split[3], out train.Sigma);
                if (split.Length > 4) int.TryParse(split[4], out train.Color);
                TypeData.Add(id, train);
            }

            //ダイヤグラムを入力する
            //例:0700:Test
            foreach (string line in Text.Read(@"Data\Train.txt"))
            {
                string[] split = line.Split(':');
                if (!int.TryParse(split[0], out int t)) continue;
                string id = split[1];
                DateTime time = new DateTime().AddHours(t / 100).AddMinutes(t % 100);
                TrainData.Add((time, id));
            }

            base.Enable();
        }

        //描画
        public override void Draw()
        {
            //Y軸
            for (int i = 0; i < 40; i++)
            {
                bool isfive = i % 5 == 0;
                double value = Average * i / 10;
                Drawing.Line(96, 640 - 30 * i, 1088, 0, isfive ? 0xcccccc : 0x606060, isfive ? 2 : 1);
                if (i % 10 == 0) Drawing.Text(64, 640 - 8 - 30 * i, value, i == 10 ? 0xffff00 : 0xffffff);
            }
            if (TimeData.Count == 0) return;
            for (int i = 0; i <= 90; i++)
            {
                DateTime time = TimeData[0].Time.AddMinutes(i);
                double basevalue = GetBasePeople(time);
                double value = GetBasePeople(time) + GetAddPeople(time);
                //Drawing.Box(96 + 12 * i, 641, 8, -value * 6, 0xff8000);
                Drawing.Box(96 + 12 * i, 641, 8, -basevalue * 6, 0x00ffff);
                if (time.Minute % 5 == 0) Drawing.Text(96 + 12 * i, 641, $"{time:t}");
                foreach (var train in TrainData)
                {
                    if (train.Time == time)
                    {
                        var type = TypeData[train.ID];
                        Drawing.Text(96 + 12 * i, 661, $"{type.Name}", type.Color);
                        break;
                    }
                }
            }
            base.Draw();
        }

        //処理
        public override void Update()
        {
            //終了
            if (Key.IsPushed(EKey.Esc)) DXLib.End();
            base.Update();
        }

        public static double GetBasePeople(DateTime time)
        {
            double value = 0;
            for (int i = 0; i < TimeData.Count; i++)
            {
                var data = TimeData[i];
                //次のデータが存在しない場合
                if (i == TimeData.Count - 1)
                {
                    return data.Value;
                }
                //次のデータが存在する場合
                else
                {
                    var next = TimeData[i + 1];
                    value = data.Value;//そのまま(左上)
                    value = Easing.Ease((time - data.Time).Minutes, (next.Time - data.Time).Minutes,
                        data.Value, next.Value, EEasing.Linear, EInOut.InOut);//直線化(右上)
                    value = Easing.Ease((time - data.Time).Minutes, (next.Time - data.Time).Minutes,
                        data.Value, next.Value, EEasing.Sine, EInOut.InOut);//スムージング(下)
                    //データが最終かを検知
                    if (time < next.Time) return value;
                }
            }
            return value;
        }

        public static double GetAddPeople(DateTime time)
        {
            double odd = GetBasePeople(time);
            double value = 0;
            foreach (var data in TrainData)
            {
                var type = TypeData[data.ID];
                double until = (data.Time - time).TotalMinutes;
                value += odd * Value(type.Mu, type.Sigma, until == 0 ? 0.01 : until);
            }
            return value;
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

    public class Type
    {
        public string Name = "";
        public double Mu, Sigma;
        public int Color = 0xffffff;
    }
}