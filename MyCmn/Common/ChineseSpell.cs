using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Text;

namespace MyCmn
{
    /// <summary>
    /// 实现汉字转化为拼音
    /// <remarks>实现的原理就是先将汉字转化成为内码，然后通过内码和拼音的对照来查找</remarks>
    /// </summary>
    public class ChineseSpell
    {
        private static Encoding gb2312 = Encoding.GetEncoding("GB2312");
        private static Regex MyRegex = new Regex("^[一-龥]$"); //汉字的正则表达式.eg: if(MyRegex.IsMatch(chrstr.ToString()))
        private static int[] pyvalue = new int[]{
-20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,-20032,-20026, 
-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,-19756,-19751,-19746,-19741,-19739,-19728, 
-19725,-19715,-19540,-19531,-19525,-19515,-19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263, 
-19261,-19249,-19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,-19003,-18996, 
-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,-18731,-18722,-18710,-18697,-18696,-18526, 
-18518,-18501,-18490,-18478,-18463,-18448,-18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, 
-18181,-18012,-17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,-17733,-17730, 
-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,-17468,-17454,-17433,-17427,-17417,-17202, 
-17185,-16983,-16970,-16942,-16915,-16733,-16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459, 
-16452,-16448,-16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,-16212,-16205, 
-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,-15933,-15920,-15915,-15903,-15889,-15878, 
-15707,-15701,-15681,-15667,-15661,-15659,-15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416, 
-15408,-15394,-15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,-15149,-15144, 
-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,-14941,-14937,-14933,-14930,-14929,-14928, 
-14926,-14922,-14921,-14914,-14908,-14902,-14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668, 
-14663,-14654,-14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,-14170,-14159, 
-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,-14109,-14099,-14097,-14094,-14092,-14090, 
-14087,-14083,-13917,-13914,-13910,-13907,-13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658, 
-13611,-13601,-13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,-13340,-13329, 
-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,-13068,-13063,-13060,-12888,-12875,-12871, 
-12860,-12858,-12852,-12849,-12838,-12831,-12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346, 
-12320,-12300,-12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,-11781,-11604, 
-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,-11055,-11052,-11045,-11041,-11038,-11024, 
-11020,-11019,-11018,-11014,-10838,-10832,-10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331, 
-10329,-10328,-10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254
        };
        private static string[] pystr = new string[]{
            "A","Ai","An","Ang","Ao",
            "Ba","Bai","Ban","Bang","Bao","Bei","Ben","Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu",
            "Ca","Cai","Can","Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng","Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong","Cou","Cu","Cuan","Cui","Cun","Cuo",
            "Da","Dai","Dan","Dang","Dao","De","Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui","Dun","Duo",
            "E","En","Er",
            "Fa","Fan","Fang","Fei","Fen","Feng","Fo","Fou","Fu",
            "Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong","Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo",
            "Ha","Hai","Han","Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan","Huang","Hui","Hun","Huo",
            "Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing","Jiong","Jiu","Ju","Juan","Jue","Jun",
            "Ka","Kai","Kan","Kang","Kao","Ke","Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
            "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang","Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun","Luo",
            "Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian","Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu",
            "Na","Nai","Nan","Nang","Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning","Niu","Nong","Nu","Nv","Nuan","Nue","Nuo",
            "O","Ou",
            "Pa","Pai","Pan","Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po","Pu",
            "Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu","Quan","Que","Qun",
            "Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou","Ru","Ruan","Rui","Run","Ruo",
            "Sa","Sai","San","Sang","Sao","Se","Sen","Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu","Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan","Sui","Sun","Suo",
            "Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian","Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo",
            "Wa","Wai","Wan","Wang","Wei","Wen","Weng","Wo","Wu",
            "Xi","Xia","Xian","Xiang","Xiao","Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun",
            "Ya","Yan","Yang","Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
            "Za","Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan","Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan","Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
        };

        private static string OnlyUpperFirstChar(string Value)
        {
            if (string.IsNullOrEmpty(Value)) return string.Empty;

            return Value.Slice(0, 1).ToUpper() + Value.Slice(1).ToLower();
        }
        /// <summary>
        /// 将汉字转化为全拼
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static string ConvertToAllSpell(string strChinese)
        {
            Regex MyRegex = new Regex("^[一-龥]$");
            byte[] array = new byte[2];
            var returnstr = new List<string>();
            int chrasc = 0;
            int i1 = 0;
            int i2 = 0;
            char[] nowchar = strChinese.ToCharArray();
            for (int j = 0; j < nowchar.Length; j++)
            {
                if (MyRegex.IsMatch(nowchar[j].ToString()))
                {
                    array = gb2312.GetBytes(nowchar[j].ToString());
                    i1 = (short)(array[0]);
                    i2 = (short)(array[1]);
                    chrasc = i1 * 256 + i2 - 65536;
                    if (chrasc > 0 && chrasc < 160)
                    {
                        returnstr.Add(nowchar[j].ToString().ToUpper());
                    }
                    else
                    {
                        for (int i = (pyvalue.Length - 1); i >= 0; i--)
                        {
                            if (pyvalue[i] <= chrasc)
                            {
                                returnstr.Add(OnlyUpperFirstChar(pystr[i]));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    returnstr.Add(nowchar[j].ToString().ToUpper());
                }
            }
            return returnstr.Join(" ");
        }
        /// <summary>
        /// 将汉字转化为拼音首字母（大写）
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static string GetFirstSpell(string strChinese)
        {
            int len = strChinese.Length;
            string myStr = "";
            for (int i = 0; i < len; i++)
            {
                myStr += GetFirstSpell(strChinese[i]);
            }
            return myStr;
        }
        /// <summary>
        /// 获取第一个汉字的首字母（大写）；
        /// </summary>
        /// <param name="charChinese"></param>
        /// <returns></returns>
        private static string GetFirstSpell(char charChinese)
        {
            byte[] arrCN = gb2312.GetBytes(new char[] { charChinese });
            if (arrCN.Length > 1)
            {
                int area = (short)arrCN[0];
                int pos = (short)arrCN[1];
                int code = (area << 8) + pos;
                int[] areacode = { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481 };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25) max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                    {
                        byte[] bytes = new byte[] { (byte)(65 + i) };
                        return gb2312.GetString(bytes, 0, bytes.Length);
                    }
                }
                return "*";
            }
            else return charChinese.ToString();
        }
    }
}