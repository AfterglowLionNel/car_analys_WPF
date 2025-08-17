using CsvHelper.Configuration;
using System.Globalization;

namespace CarAnalysisDashboard.Models
{
    public class CarDataCsvMap : ClassMap<CarData>
    {
        public CarDataCsvMap()
        {
            Map(m => m.Name).Name("車種名");
            Map(m => m.Grade).Name("グレード");
            Map(m => m.Price).Name("支払総額").Convert(row =>
            {
                var value = row.Row.GetField("支払総額") ?? "";
                return ParsePrice(value);
            });
            Map(m => m.Year).Name("年式").Convert(row =>
            {
                var value = row.Row.GetField("年式") ?? "";
                return ParseYear(value);
            });
            Map(m => m.Mileage).Name("走行距離").Convert(row =>
            {
                var value = row.Row.GetField("走行距離") ?? "";
                return ParseMileage(value);
            });
            Map(m => m.Transmission).Name("ミッション");
            Map(m => m.HasRepairHistory).Name("修復歴").Convert(row =>
            {
                var value = row.Row.GetField("修復歴") ?? "";
                return value.Contains("あり");
            });
            // CSVに存在するフィールドをマッピング
            Map(m => m.Model).Name("モデル");
            Map(m => m.DetailUrl).Name("車両URL");
            Map(m => m.SourceUrl).Name("ソースURL");
            Map(m => m.AcquisitionDateTime).Name("取得日時");
            Map(m => m.AcquisitionDate).Name("取得日");
            Map(m => m.AcquisitionTime).Name("取得時刻");
            Map(m => m.EngineCapacity).Name("排気量");
            // CSVに存在しないフィールドはIgnoreする
            Map(m => m.Color).Ignore();
            Map(m => m.Location).Ignore();
            Map(m => m.FuelType).Ignore();
            Map(m => m.DriveType).Ignore();
            Map(m => m.BodyType).Ignore();
            Map(m => m.Doors).Ignore();
            Map(m => m.Seats).Ignore();
            Map(m => m.Inspection).Ignore();
            Map(m => m.Warranty).Ignore();
            Map(m => m.RecyclingFee).Ignore();
            Map(m => m.LegalMaintenance).Ignore();
            Map(m => m.Dealer).Ignore();
            Map(m => m.Comments).Ignore();
            Map(m => m.DataDate).Ignore();
            Map(m => m.CarModel).Ignore();
            Map(m => m.Id).Ignore();
        }

        private static long ParsePrice(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            
            var originalValue = value;
            
            // より積極的なクリーニング - 全角・半角文字、隠し文字、スペースなどを除去
            value = value.Replace("万円", "")
                        .Replace("円", "")
                        .Replace(",", "")  // 半角カンマ
                        .Replace("，", "")  // 全角カンマ  
                        .Replace("￥", "")
                        .Replace("¥", "")
                        .Replace("　", "")  // 全角スペース
                        .Replace(" ", "")   // 半角スペース
                        .Replace("\t", "")  // タブ
                        .Replace("\r", "")  // キャリッジリターン
                        .Replace("\n", "")  // ニューライン
                        .Replace("'", "")   // アポストロフィ（Excelの文字列プレフィックス）
                        .Replace(((char)160).ToString(), "") // 不可視スペース CHAR(160)
                        .Replace("０", "0") // 全角0
                        .Replace("１", "1") // 全角1
                        .Replace("２", "2") // 全角2
                        .Replace("３", "3") // 全角3
                        .Replace("４", "4") // 全角4
                        .Replace("５", "5") // 全角5
                        .Replace("６", "6") // 全角6
                        .Replace("７", "7") // 全角7
                        .Replace("８", "8") // 全角8
                        .Replace("９", "9") // 全角9
                        .Replace("．", ".") // 全角ピリオド
                        .Trim();

            // 万単位かどうかを判定（元の値から）
            bool hadManUnit = originalValue.Contains("万");
            
            // 数値以外の文字を除去（ピリオドと数字以外）
            var cleanValue = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d\.]", "");
            
            if (decimal.TryParse(cleanValue, out var price))
            {
                // 万円単位だった場合のみ10000を掛ける
                if (hadManUnit)
                {
                    price *= 10000;
                }
                
                // 整数に変換して確実な比較を保証
                return (long)System.Math.Round(price);
            }
            
            // パースに失敗した場合のみログ出力
            System.Diagnostics.Debug.WriteLine($"価格パース失敗: '{originalValue}' を数値に変換できませんでした");
            return 0;
        }

        private static int ParseYear(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            
            // "2025(R07)" 形式の場合、最初の数値部分を抽出
            if (value.Contains("("))
            {
                value = value.Split('(')[0];
            }
            
            value = value.Replace("年", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("H", "")
                        .Replace("R", "")
                        .Replace("S", "")
                        .Trim();
            
            if (value.Contains("/"))
            {
                value = value.Split('/')[0];
            }
            
            if (int.TryParse(value, out var year))
            {
                if (year < 100)
                {
                    year = year + (year <= 30 ? 2000 : 1900);
                }
                else if (year < 1000)
                {
                    year += 1988;
                }
                return year;
            }
            return 0;
        }

        private static int ParseMileage(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            
            var originalValue = value;
            bool hasMan = value.Contains("万");
            
            // より堅牢なクリーニング（価格パーサーと同様）
            value = value.Replace("万km", "")
                        .Replace("km", "")
                        .Replace("万", "")
                        .Replace(",", "")  // 半角カンマ
                        .Replace("，", "")  // 全角カンマ
                        .Replace("　", "")  // 全角スペース
                        .Replace(" ", "")   // 半角スペース
                        .Replace("'", "")   // アポストロフィ
                        .Replace(((char)160).ToString(), "") // 不可視スペース
                        .Replace("０", "0") // 全角数字
                        .Replace("１", "1")
                        .Replace("２", "2")
                        .Replace("３", "3")
                        .Replace("４", "4")
                        .Replace("５", "5")
                        .Replace("６", "6")
                        .Replace("７", "7")
                        .Replace("８", "8")
                        .Replace("９", "9")
                        .Replace("．", ".") // 全角ピリオド
                        .Trim();
            
            // 数値以外を除去
            var cleanValue = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d\.]", "");
            
            if (decimal.TryParse(cleanValue, out var mileage))
            {
                if (hasMan)
                {
                    mileage *= 10000;
                }
                return (int)mileage;
            }
            
            // パースに失敗した場合のログ
            if (!string.IsNullOrEmpty(originalValue))
            {
                System.Diagnostics.Debug.WriteLine($"走行距離パース失敗: '{originalValue}' を数値に変換できませんでした");
            }
            return 0;
        }

        private static int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            
            value = value.Replace("ドア", "")
                        .Replace("人", "")
                        .Replace("名", "")
                        .Trim();
            
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }
    }
}