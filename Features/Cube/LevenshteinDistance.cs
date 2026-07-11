using System;
using System.Collections.Generic;
using System.Text;

namespace WinFormsApp1
{
    public static class LevenshteinDistance
    {
        /// <summary>
        /// 计算两个字符串之间的编辑距离（Levenshtein Distance）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="target">目标字符串</param>
        /// <returns>编辑距离（值越小表示越相似）</returns>
        public static int Compute(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;
            if (string.IsNullOrEmpty(target))
                return source.Length;

            int sourceLength = source.Length;
            int targetLength = target.Length;

            // 创建距离矩阵
            int[,] distance = new int[sourceLength + 1, targetLength + 1];

            // 初始化第一行和第一列
            for (int i = 0; i <= sourceLength; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= targetLength; j++)
                distance[0, j] = j;

            // 计算距离
            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(
                            distance[i - 1, j] + 1,      // 删除操作
                            distance[i, j - 1] + 1       // 插入操作
                        ),
                        distance[i - 1, j - 1] + cost    // 替换操作
                    );
                }
            }

            return distance[sourceLength, targetLength];
        }

        /// <summary>
        /// 计算字符串相似度（百分比）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="target">目标字符串</param>
        /// <returns>相似度百分比（0-100）</returns>
        public static double ComputeSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 100.0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;

            int distance = Compute(source, target);
            int maxLength = Math.Max(source.Length, target.Length);

            if (maxLength == 0)
                return 100.0;

            return (1.0 - (double)distance / maxLength) * 100.0;
        }

        /// <summary>
        /// 判断两个字符串是否相似（基于阈值）
        /// </summary>
        public static bool IsSimilar(string source, string target, double similarityThreshold = 80.0)
        {
            return ComputeSimilarity(source, target) >= similarityThreshold;
        }
    }
}
