using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeBox.Compilation
{
    internal static class Extensions
    {
        public static Type GetItemType<T>(this IEnumerable<T> enumerable)
        {
            return typeof(T);
        }

        public static string UnEscape(this string s)
        {
            StringBuilder sb = new StringBuilder();
            Regex r = new Regex("\\\\[abfnrtv?\"'\\\\]|\\\\[0-3]?[0-7]{1,2}|\\\\u[0-9a-fA-F]{4}|.");
            MatchCollection mc = r.Matches(s, 0);

            foreach (Match m in mc)
            {
                if (m.Length == 1)
                {
                    sb.Append(m.Value);
                }
                else
                {
                    if (m.Value[1] >= '0' && m.Value[1] <= '7')
                    {
                        int i = 0;

                        for (int j = 1; j < m.Length; j++)
                        {
                            i *= 8;
                            i += m.Value[j] - '0';
                        }

                        sb.Append((char)i);
                    }
                    else if (m.Value[1] == 'u')
                    {
                        int i = 0;

                        for (int j = 2; j < m.Length; j++)
                        {
                            i *= 16;

                            if (m.Value[j] >= '0' && m.Value[j] <= '9')
                            {
                                i += m.Value[j] - '0';
                            }
                            else if (m.Value[j] >= 'A' && m.Value[j] <= 'F')
                            {
                                i += m.Value[j] - 'A' + 10;
                            }
                            else if (m.Value[j] >= 'a' && m.Value[j] <= 'f')
                            {
                                i += m.Value[j] - 'a' + 10;
                            }
                        }

                        sb.Append((char)i);
                    }
                    else
                    {
                        switch (m.Value[1])
                        {
                            case 'a':
                                sb.Append('\a');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'v':
                                sb.Append('\v');
                                break;
                            default:
                                sb.Append(m.Value[1]);
                                break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
