using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WinFormsApp1
{
    public class GameAttributeParser
    {
        /// <summary>
        /// 解析游戏属性字符串（英文+数字的交替组合）
        /// 示例: "HP100MP50Attack15.5Defense8.7"
        /// </summary>
        public static List<GameAttribute> ParseAttributesRegex(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<GameAttribute>();

            // 正则表达式匹配：英文单词 + 数字（整数或小数）
            // 解释：
            // ([a-zA-Z]+)    - 匹配一个或多个英文字母（属性名）
            // ([\d\.]+)      - 匹配一个或多个数字或小数点（属性值）
            string pattern = @"([A-Z])([^+\s]+)\s*([+-])(\d+%)";
            var matches = Regex.Matches(input, pattern);

            var attributes = new List<GameAttribute>();

            foreach (Match match in matches)
            { // 应该有两个捕获组：属性名和属性值
                {
                    string name = match.Groups[2].Value;
                    string valueStr = match.Groups[4].Value;

                    // 尝试解析为数值
                    {
                        attributes.Add(new GameAttribute
                        {
                            Name = name,
                            Value = valueStr,
                            OriginalString = match.Value,
                        });
                    }
                }
            }

            return attributes;
        }

        public bool CanMatchElementsWithRegexReplace(string A, string[] B)
        {
            string temp = A;

            foreach (string element in B)
            {
                // 使用正则表达式查找第一个匹配项
                string pattern = Regex.Escape(element);
                Match match = Regex.Match(temp, pattern);

                if (!match.Success)
                {

                    return false;
                }

                // 移除第一个匹配项
                temp = temp.Remove(match.Index, match.Length);
            }

            return true;
        }
    }
}
