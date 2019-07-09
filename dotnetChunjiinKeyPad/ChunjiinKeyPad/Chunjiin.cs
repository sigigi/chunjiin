using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChunjiinKeyPad
{
    public class Chunjiin
    {
        private const int HANGUL = 0;
        private const int UPPER_ENGLISH = 1;
        private const int ENGLISH = 2;
        private const int NUMBER = 3;

        private int keyInputTimerPeriod = 1000; // 1sec
        private System.Threading.Timer _keyInputTimer;

        private Button[] btn;
        private TextBox et;
        private int now_mode = HANGUL;
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtualKey);
        [DllImport("VKB.dll", CharSet = CharSet.Auto)]
        static extern void InitHook(IntPtr hHandle);
        [DllImport("VKB.dll", CharSet = CharSet.Auto)]

        static extern void InstallHook();

        private Hangul hangul = new Hangul();

        private String engnum = "";
        private bool flag_initengnum = false;
        private bool flag_engdelete = false;
        private bool flag_upper = true;

        public int KeyInputTimerPeriod { get => keyInputTimerPeriod; set => keyInputTimerPeriod = value; }

        private void OnKeyPadInputInitialize()
        {
            OnKeyPadInputTimerStop();
            _keyInputTimer = null;
        }
        public void OnKeyPadInputTimerStart(int period)
        {
            if (_keyInputTimer != null)
                _keyInputTimer.Change(period, 0);
            else
                _keyInputTimer = new System.Threading.Timer(new TimerCallback(CallbackTimerStatusUpdateKey), null, period, 0);
        }
        public void OnKeyPadInputTimerStop()
        {
            if (_keyInputTimer != null)
                _keyInputTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private void CallbackTimerStatusUpdateKey(Object stateInfo)
        {
            OnKeyPadInputInitialize();
            hangul.init();
        }

        public class Hangul
        {
            public String chosung = "";
            public String jungsung = "";
            public String jongsung = "";
            public String jongsung2 = "";
            public int step = 0;
            public bool flag_writing = false;
            public bool flag_dotused = false;
            public bool flag_doubled = false;
            public bool flag_addcursor = false;
            public bool flag_space = false;
            public void init()
            {
                this.chosung = "";
                this.jungsung = "";
                this.jongsung = "";
                this.jongsung2 = "";
                this.step = 0;
                this.flag_writing = false;
                this.flag_dotused = false;
                this.flag_doubled = false;
                this.flag_addcursor = false;
                this.flag_space = false;
            }
        }

        public Chunjiin(TextBox editText, Button[] bt)
        {
            et = editText;
            et.Click += Et_Click;
            setButton(bt);
        }

        private void Et_Click(object sender, EventArgs e)
        {
            hangul.init();
            init_engnum();
        }
        private void init_engnum()
        {
            engnum = "";
            flag_initengnum = false;
            flag_engdelete = false;
        }

        private void setButton(Button[] inputbtn)
        {
            btn = inputbtn;
            for (int i = 0; i < 12; i++)
                btn[i].Click += Chunjiin_Click;

            btn[12].Click += Chunjiin_ClickKeyChange;
            setBtnText(now_mode);
        }
        private void setBtnText(int mode)
        {
            switch (mode)
            {
                case HANGUL:
                    btn[0].Text = "ㅇㅁ";
                    btn[1].Text = "ㅣ";
                    btn[2].Text = "·";
                    btn[3].Text = "ㅡ";
                    btn[4].Text = "ㄱㅋ";
                    btn[5].Text = "ㄴㄹ";
                    btn[6].Text = "ㄷㅌ";
                    btn[7].Text = "ㅂㅍ";
                    btn[8].Text = "ㅅㅎ";
                    btn[9].Text = "ㅈㅊ";
                    break;
                case UPPER_ENGLISH:
                    btn[0].Text = "@?!";
                    btn[1].Text = ".QZ";
                    btn[2].Text = "ABC";
                    btn[3].Text = "DEF";
                    btn[4].Text = "GHI";
                    btn[5].Text = "JKL";
                    btn[6].Text = "MNO";
                    btn[7].Text = "PRS";
                    btn[8].Text = "TUV";
                    btn[9].Text = "WXY";
                    flag_upper = true;
                    break;
                case ENGLISH:
                    btn[0].Text = "@?!";
                    btn[1].Text = ".qz";
                    btn[2].Text = "abc";
                    btn[3].Text = "def";
                    btn[4].Text = "ghi";
                    btn[5].Text = "jkl";
                    btn[6].Text = "mno";
                    btn[7].Text = "prs";
                    btn[8].Text = "tuv";
                    btn[9].Text = "wxy";
                    flag_upper = false;
                    break;
                case NUMBER:
                    for (int i = 0; i < 10; i++)
                        btn[i].Text = i.ToString();
                    break;
            }
        }
        private void Chunjiin_ClickKeyChange(object sender, EventArgs e)
        {
            now_mode = (now_mode == NUMBER) ? HANGUL : now_mode + 1;
            setBtnText(now_mode);
            hangul.init();
            init_engnum();
        }

        private void Chunjiin_Click(object sender, EventArgs e)
        {
            OnKeyPadInputTimerStop();
            int input = -1;
            for(int i = 0; i < btn.Length; i++)
            {
                if (btn[i] == ((Button)sender))
                {
                    input = i;
                }
            }
            if (input == -1)
                return;
                
            if (now_mode == HANGUL)
                hangulMake(input);
            else if ((now_mode == ENGLISH || now_mode == UPPER_ENGLISH))
                engMake(input);
            else // if(now_mode == NUMBER)
                numMake(input);

            write(now_mode);
            et.Focus();
            OnKeyPadInputTimerStart(keyInputTimerPeriod);
        }
        private void hangulMake(int input)
        {
            String beforedata = "";
            String nowdata = "";
            String overdata = "";

            if (et.SelectionLength > 0) // Delete selection content in textbox // bjh
            {
                delete();
            }

            if (input == 10) //띄어쓰기 // space bar
            {
                if (hangul.flag_writing)
                    hangul.init();
                else
                    hangul.flag_space = true;
            }
            else if (input == 11) //지우기 // delete
            {
                if (hangul.step == 0)
                {
                    if (hangul.chosung.Length == 0)
                    {
                        delete();
                        hangul.flag_writing = false;
                    }
                    else
                        hangul.chosung = "";
                }
                else if (hangul.step == 1)
                {
                    if (hangul.jungsung.Equals("·") || hangul.jungsung.Equals("‥"))
                    {
                        delete();
                        if (hangul.chosung.Length == 0)
                            hangul.flag_writing = false;
                    }
                    hangul.jungsung = "";
                    hangul.step = 0;
                }
                else if (hangul.step == 2)
                {
                    hangul.jongsung = "";
                    hangul.step = 1;
                }
                else if (hangul.step == 3)
                {
                    hangul.jongsung2 = "";
                    hangul.step = 2;
                }
            }
            else if (input == 1 || input == 2 || input == 3) //모음
            {
                //받침에서 떼어오는거 추가해야함
                bool batchim = false;
                if (hangul.step == 2)
                {
                    delete();
                    String s = hangul.jongsung;
                    //bug fixed, 16.4.22 ~
                    if (!hangul.flag_doubled)
                    {
                        hangul.jongsung = "";
                        hangul.flag_writing = false;
                        write(now_mode);
                    }
                    // ~ bug fixed, 16.4.22
                    hangul.init();
                    hangul.chosung = s;
                    hangul.step = 0;
                    batchim = true;
                }
                else if (hangul.step == 3)
                {
                    String s = hangul.jongsung2;
                    if (hangul.flag_doubled)
                        delete();
                    else
                    {
                        delete();
                        hangul.jongsung2 = "";
                        hangul.flag_writing = false;
                        write(now_mode);
                    }
                    hangul.init();
                    hangul.chosung = s;
                    hangul.step = 0;
                    batchim = true;
                }
                beforedata = hangul.jungsung;
                hangul.step = 1;
                if (input == 1) // ㅣ ㅓ ㅕ ㅐ ㅔ ㅖㅒ ㅚ ㅟ ㅙ ㅝ ㅞ ㅢ
                {
                    if (beforedata.Length == 0) nowdata = "ㅣ";
                    else if (beforedata.Equals("·"))
                    {
                        nowdata = "ㅓ";
                        hangul.flag_dotused = true;
                    }
                    else if (beforedata.Equals("‥"))
                    {
                        nowdata = "ㅕ";
                        hangul.flag_dotused = true;
                    }
                    else if (beforedata.Equals("ㅏ")) nowdata = "ㅐ";
                    else if (beforedata.Equals("ㅑ")) nowdata = "ㅒ";
                    else if (beforedata.Equals("ㅓ")) nowdata = "ㅔ";
                    else if (beforedata.Equals("ㅕ")) nowdata = "ㅖ";
                    else if (beforedata.Equals("ㅗ")) nowdata = "ㅚ";
                    else if (beforedata.Equals("ㅜ")) nowdata = "ㅟ";
                    else if (beforedata.Equals("ㅠ")) nowdata = "ㅝ";
                    else if (beforedata.Equals("ㅘ")) nowdata = "ㅙ";
                    else if (beforedata.Equals("ㅝ")) nowdata = "ㅞ";
                    else if (beforedata.Equals("ㅡ")) nowdata = "ㅢ";
                    else
                    {
                        hangul.init();
                        hangul.step = 1;
                        nowdata = "ㅣ";
                    }
                }
                else if (input == 2) // ·,‥,ㅏ,ㅑ,ㅜ,ㅠ,ㅘ
                {
                    if (beforedata.Length == 0)
                    {
                        nowdata = "·";
                        if (batchim)
                            hangul.flag_addcursor = true;
                    }
                    else if (beforedata.Equals("·"))
                    {
                        nowdata = "‥";
                        hangul.flag_dotused = true;
                    }
                    else if (beforedata.Equals("‥"))
                    {
                        nowdata = "·";
                        hangul.flag_dotused = true;
                    }
                    else if (beforedata.Equals("ㅣ")) nowdata = "ㅏ";
                    else if (beforedata.Equals("ㅏ")) nowdata = "ㅑ";
                    else if (beforedata.Equals("ㅑ")) nowdata = "ㅏ"; // bjh 
                    else if (beforedata.Equals("ㅡ")) nowdata = "ㅜ";
                    else if (beforedata.Equals("ㅜ")) nowdata = "ㅠ";
                    else if (beforedata.Equals("ㅠ")) nowdata = "ㅜ"; // bjh
                    else if (beforedata.Equals("ㅚ")) nowdata = "ㅘ";
                    else
                    {
                        hangul.init();
                        hangul.step = 1;
                        nowdata = "·";
                    }
                }
                else if (input == 3) // ㅡ, ㅗ, ㅛ
                {
                    if (beforedata.Length == 0) nowdata = "ㅡ";
                    else if (beforedata.Equals("·"))
                    {
                        nowdata = "ㅗ";
                        hangul.flag_dotused = true;
                    }
                    else if (beforedata.Equals("‥"))
                    {
                        nowdata = "ㅛ";
                        hangul.flag_dotused = true;
                    }
                    else
                    {
                        hangul.init();
                        hangul.step = 1;
                        nowdata = "ㅡ";
                    }
                }
                hangul.jungsung = nowdata;
            }
            else //자음
            {
                if (hangul.step == 1)
                {
                    if (hangul.jungsung.Equals("·") || hangul.jungsung.Equals("‥"))
                        hangul.init();
                    else
                        hangul.step = 2;
                }
                if (hangul.step == 0) beforedata = hangul.chosung;
                else if (hangul.step == 2) beforedata = hangul.jongsung;
                else if (hangul.step == 3) beforedata = hangul.jongsung2;

                if (input == 4) // ㄱ, ㅋ, ㄲ, ㄺ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㄱ";
                            else
                                nowdata = "ㄱ";
                        }
                        else
                            nowdata = "ㄱ";
                    }
                    else if (beforedata.Equals("ㄱ"))
                        nowdata = "ㅋ";
                    else if (beforedata.Equals("ㅋ"))
                        nowdata = "ㄲ";
                    else if (beforedata.Equals("ㄲ"))
                        nowdata = "ㄱ";
                    else if (beforedata.Equals("ㄹ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㄱ";
                    }
                    else
                        overdata = "ㄱ";
                }
                else if (input == 5) // ㄴ ㄹ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㄴ";
                            else
                                nowdata = "ㄴ";
                        }
                        else
                            nowdata = "ㄴ";
                    }
                    else if (beforedata.Equals("ㄴ"))
                        nowdata = "ㄹ";
                    else if (beforedata.Equals("ㄹ"))
                        nowdata = "ㄴ";
                    else
                        overdata = "ㄴ";
                }
                else if (input == 6) // ㄷ, ㅌ, ㄸ, ㄾ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㄷ";
                            else
                                nowdata = "ㄷ";
                        }
                        else
                            nowdata = "ㄷ";
                    }
                    else if (beforedata.Equals("ㄷ"))
                        nowdata = "ㅌ";
                    else if (beforedata.Equals("ㅌ"))
                        nowdata = "ㄸ";
                    else if (beforedata.Equals("ㄸ"))
                        nowdata = "ㄷ";
                    else if (beforedata.Equals("ㄹ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㄷ";
                    }
                    else
                        overdata = "ㄷ";
                }
                else if (input == 7) // ㅂ, ㅍ, ㅃ, ㄼ, ㄿ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㅂ";
                            else
                                nowdata = "ㅂ";
                        }
                        else
                            nowdata = "ㅂ";
                    }
                    else if (beforedata.Equals("ㅂ"))
                        nowdata = "ㅍ";
                    else if (beforedata.Equals("ㅍ"))
                        nowdata = "ㅃ";
                    else if (beforedata.Equals("ㅃ"))
                        nowdata = "ㅂ";
                    else if (beforedata.Equals("ㄹ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅂ";
                    }
                    else
                        overdata = "ㅂ";
                }
                else if (input == 8) // ㅅ, ㅎ, ㅆ, ㄳ, ㄶ, ㄽ, ㅀ, ㅄ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㅅ";
                            else
                                nowdata = "ㅅ";
                        }
                        else
                            nowdata = "ㅅ";
                    }
                    else if (beforedata.Equals("ㅅ"))
                        nowdata = "ㅎ";
                    else if (beforedata.Equals("ㅎ"))
                        nowdata = "ㅆ";
                    else if (beforedata.Equals("ㅆ"))
                        nowdata = "ㅅ";
                    else if (beforedata.Equals("ㄱ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅅ";
                    }
                    else if (beforedata.Equals("ㄴ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅅ";
                    }
                    else if (beforedata.Equals("ㄹ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅅ";
                    }
                    else if (beforedata.Equals("ㅂ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅅ";
                    }
                    else
                        overdata = "ㅅ";
                }
                else if (input == 9) // ㅈ, ㅊ, ㅉ, ㄵ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㅈ";
                            else
                                nowdata = "ㅈ";
                        }
                        else
                            nowdata = "ㅈ";
                    }
                    else if (beforedata.Equals("ㅈ"))
                        nowdata = "ㅊ";
                    else if (beforedata.Equals("ㅊ"))
                        nowdata = "ㅉ";
                    else if (beforedata.Equals("ㅉ"))
                        nowdata = "ㅈ";
                    else if (beforedata.Equals("ㄴ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅈ";
                    }
                    else
                        overdata = "ㅈ";
                }
                else if (input == 0) // ㅇ, ㅁ, ㄻ
                {
                    if (beforedata.Length == 0)
                    {
                        if (hangul.step == 2)
                        {
                            if (hangul.chosung.Length == 0)
                                overdata = "ㅇ";
                            else
                                nowdata = "ㅇ";
                        }
                        else
                            nowdata = "ㅇ";
                    }
                    else if (beforedata.Equals("ㅇ"))
                        nowdata = "ㅁ";
                    else if (beforedata.Equals("ㅁ"))
                        nowdata = "ㅇ";
                    else if (beforedata.Equals("ㄹ") && hangul.step == 2)
                    {
                        hangul.step = 3;
                        nowdata = "ㅇ";
                    }
                    else
                        overdata = "ㅇ";
                }

                if (nowdata.Length > 0)
                {
                    if (hangul.step == 0)
                        hangul.chosung = nowdata;
                    else if (hangul.step == 2)
                        hangul.jongsung = nowdata;
                    else //if(hangul.step == 3)
                        hangul.jongsung2 = nowdata;
                }
                if (overdata.Length > 0)
                {
                    hangul.flag_writing = false;
                    hangul.init();
                    hangul.chosung = overdata;
                }
            }

        }

        private int getUnicode(String real_jong)
        {
            int cho, jung, jong;
            //초성
            if (hangul.chosung.Length == 0)
            {
                if (hangul.jungsung.Length == 0 || hangul.jungsung.Equals("·") || hangul.jungsung.Equals("‥"))
                    return 0;
            }

            if (hangul.chosung.Equals("ㄱ")) cho = 0;
            else if (hangul.chosung.Equals("ㄲ")) cho = 1;
            else if (hangul.chosung.Equals("ㄴ")) cho = 2;
            else if (hangul.chosung.Equals("ㄷ")) cho = 3;
            else if (hangul.chosung.Equals("ㄸ")) cho = 4;
            else if (hangul.chosung.Equals("ㄹ")) cho = 5;
            else if (hangul.chosung.Equals("ㅁ")) cho = 6;
            else if (hangul.chosung.Equals("ㅂ")) cho = 7;
            else if (hangul.chosung.Equals("ㅃ")) cho = 8;
            else if (hangul.chosung.Equals("ㅅ")) cho = 9;
            else if (hangul.chosung.Equals("ㅆ")) cho = 10;
            else if (hangul.chosung.Equals("ㅇ")) cho = 11;
            else if (hangul.chosung.Equals("ㅈ")) cho = 12;
            else if (hangul.chosung.Equals("ㅉ")) cho = 13;
            else if (hangul.chosung.Equals("ㅊ")) cho = 14;
            else if (hangul.chosung.Equals("ㅋ")) cho = 15;
            else if (hangul.chosung.Equals("ㅌ")) cho = 16;
            else if (hangul.chosung.Equals("ㅍ")) cho = 17;
            else /*if ( hangul.chosung.Equals("ㅎ"))*/	cho = 18;

            if (hangul.jungsung.Length == 0 && hangul.jongsung.Length == 0)
                return 0x1100 + cho;
            if (hangul.jungsung.Equals("·") || hangul.jungsung.Equals("‥"))
                return 0x1100 + cho;

            // 중성
            if (hangul.jungsung.Equals("ㅏ")) jung = 0;
            else if (hangul.jungsung.Equals("ㅐ")) jung = 1;
            else if (hangul.jungsung.Equals("ㅑ")) jung = 2;
            else if (hangul.jungsung.Equals("ㅒ")) jung = 3;
            else if (hangul.jungsung.Equals("ㅓ")) jung = 4;
            else if (hangul.jungsung.Equals("ㅔ")) jung = 5;
            else if (hangul.jungsung.Equals("ㅕ")) jung = 6;
            else if (hangul.jungsung.Equals("ㅖ")) jung = 7;
            else if (hangul.jungsung.Equals("ㅗ")) jung = 8;
            else if (hangul.jungsung.Equals("ㅘ")) jung = 9;
            else if (hangul.jungsung.Equals("ㅙ")) jung = 10;
            else if (hangul.jungsung.Equals("ㅚ")) jung = 11;
            else if (hangul.jungsung.Equals("ㅛ")) jung = 12;
            else if (hangul.jungsung.Equals("ㅜ")) jung = 13;
            else if (hangul.jungsung.Equals("ㅝ")) jung = 14;
            else if (hangul.jungsung.Equals("ㅞ")) jung = 15;
            else if (hangul.jungsung.Equals("ㅟ")) jung = 16;
            else if (hangul.jungsung.Equals("ㅠ")) jung = 17;
            else if (hangul.jungsung.Equals("ㅡ")) jung = 18;
            else if (hangul.jungsung.Equals("ㅢ")) jung = 19;
            else /*if ( hangul.jungsung.Equals("ㅣ"))*/	jung = 20;

            if (hangul.chosung.Length == 0 && hangul.jongsung.Length == 0)
                return 0x1161 + jung;

            // 종성
            if (real_jong.Length == 0) jong = 0;
            else if (real_jong.Equals("ㄱ")) jong = 1;
            else if (real_jong.Equals("ㄲ")) jong = 2;
            else if (real_jong.Equals("ㄳ")) jong = 3;
            else if (real_jong.Equals("ㄴ")) jong = 4;
            else if (real_jong.Equals("ㄵ")) jong = 5;
            else if (real_jong.Equals("ㄶ")) jong = 6;
            else if (real_jong.Equals("ㄷ")) jong = 7;
            else if (real_jong.Equals("ㄹ")) jong = 8;
            else if (real_jong.Equals("ㄺ")) jong = 9;
            else if (real_jong.Equals("ㄻ")) jong = 10;
            else if (real_jong.Equals("ㄼ")) jong = 11;
            else if (real_jong.Equals("ㄽ")) jong = 12;
            else if (real_jong.Equals("ㄾ")) jong = 13;
            else if (real_jong.Equals("ㄿ")) jong = 14;
            else if (real_jong.Equals("ㅀ")) jong = 15;
            else if (real_jong.Equals("ㅁ")) jong = 16;
            else if (real_jong.Equals("ㅂ")) jong = 17;
            else if (real_jong.Equals("ㅄ")) jong = 18;
            else if (real_jong.Equals("ㅅ")) jong = 19;
            else if (real_jong.Equals("ㅆ")) jong = 20;
            else if (real_jong.Equals("ㅇ")) jong = 21;
            else if (real_jong.Equals("ㅈ")) jong = 22;
            else if (real_jong.Equals("ㅊ")) jong = 23;
            else if (real_jong.Equals("ㅋ")) jong = 24;
            else if (real_jong.Equals("ㅌ")) jong = 25;
            else if (real_jong.Equals("ㅍ")) jong = 26;
            else /*if ( real_jong.Equals("ㅎ"))*/	jong = 27;

            if (hangul.chosung.Length == 0 && hangul.jungsung.Length == 0)
                return 0x11a8 + jong;

            return 44032 + cho * 588 + jung * 28 + jong;
        }

        private String checkDouble(String jong, String jong2)
        {
            String s = "";
            if (jong.Equals("ㄱ"))
            {
                if (jong2.Equals("ㅅ")) s = "ㄳ";
            }
            else if (jong.Equals("ㄴ"))
            {
                if (jong2.Equals("ㅈ")) s = "ㄵ";
                else if (jong2.Equals("ㅎ")) s = "ㄶ";
            }
            else if (jong.Equals("ㄹ"))
            {
                if (jong2.Equals("ㄱ")) s = "ㄺ";
                else if (jong2.Equals("ㅁ")) s = "ㄻ";
                else if (jong2.Equals("ㅂ")) s = "ㄼ";
                else if (jong2.Equals("ㅅ")) s = "ㄽ";
                else if (jong2.Equals("ㅌ")) s = "ㄾ";
                else if (jong2.Equals("ㅍ")) s = "ㄿ";
                else if (jong2.Equals("ㅎ")) s = "ㅀ";
            }
            else if (jong.Equals("ㅂ"))
            {
                if (jong2.Equals("ㅅ")) s = "ㅄ";
            }
            return s;
        }
        private void delete() // bjh
        {
            int position = et.SelectionStart;
            if (position == 0 && et.SelectionLength == 0)
                return;

            if (et.SelectionLength == 0)
            {
                String origin = "";
                String str = "";

                origin = et.Text;
                str += origin.Substring(0, position - 1);
                str += origin.Substring(position);
                et.Text = (str);
                et.SelectionStart = (position - 1);
            }
            else
            {
                if (et.Text.Length == et.SelectionLength)
                {
                    et.Text = "";
                    et.SelectionStart = 0;
                }
                else
                {
                    int length = et.SelectionLength;
                    int start = et.SelectionStart;
                    et.Text = et.Text.Remove(et.SelectionStart, length);
                    et.SelectionStart = start;
                }
            }
        }
        private void engMake(int input)
        {
            if (input == 10) // 띄어쓰기
            {
                if (engnum.Length == 0)
                    engnum = " ";
                else
                    engnum = "";
                flag_initengnum = true;
            }
            else if (input == 11) // 지우기
            {
                delete();
                init_engnum();
            }
            else
            {
                String str = "";
                switch (input)
                {
                    case 0: str = "@?!"; break;
                    case 1: str = ".QZ"; break;
                    case 2: str = "ABC"; break;
                    case 3: str = "DEF"; break;
                    case 4: str = "GHI"; break;
                    case 5: str = "JKL"; break;
                    case 6: str = "MNO"; break;
                    case 7: str = "PRS"; break;
                    case 8: str = "TUV"; break;
                    case 9: str = "WXY"; break;
                    default: return;
                }

                char[] ch = str.ToCharArray();

                if (engnum.Length == 0)
                    engnum = ch[0].ToString();
                else if (engnum.Equals(ch[0].ToString()))
                {
                    engnum = ch[1].ToString();
                    flag_engdelete = true;
                }
                else if (engnum.Equals(ch[1].ToString()))
                {
                    engnum = ch[2].ToString();
                    flag_engdelete = true;
                }
                else if (engnum.Equals(ch[2].ToString()))
                {
                    engnum = ch[0].ToString();
                    flag_engdelete = true;
                }
                else
                    engnum = ch[0].ToString();
            }
        }
        private void numMake(int input)
        {
            if (input == 10) // 띄어쓰기
                engnum = " ";
            else if (input == 11) // 지우기
                delete();
            else
                engnum = input.ToString();

            flag_initengnum = true;
        }

        private void write(int mode)
        {
            int position = et.SelectionStart;
            String origin = "";
            String str = "";
            origin = et.Text;
            
            if (mode == HANGUL)
            {
                bool dotflag = false;
                bool doubleflag = false;
                bool spaceflag = false;
                bool impossiblejongsungflag = false;
                char unicode;
                String real_jongsung = checkDouble(hangul.jongsung, hangul.jongsung2);
                if (real_jongsung.Length == 0)
                {
                    real_jongsung = hangul.jongsung;
                    if (hangul.jongsung2.Length != 0)
                        doubleflag = true;
                }

                //bug fixed, 16.4.22 ~
                //added impossible jongsungflag.
                if (hangul.jongsung.Equals("ㅃ") || hangul.jongsung.Equals("ㅉ") || hangul.jongsung.Equals("ㄸ"))
                {
                    doubleflag = true;
                    impossiblejongsungflag = true;
                    unicode = (char)getUnicode("");
                }
                else
                    unicode = (char)getUnicode(real_jongsung);
                // ~ bug fixed, 16.4.22

                if (!hangul.flag_writing)
                    str += origin.Substring(0, position);
                else if (hangul.flag_dotused)
                {
                    if (hangul.chosung.Length == 0)
                        str += origin.Substring(0, position - 1);
                    else
                        str += origin.Substring(0, position - 2);
                }
                else if (hangul.flag_doubled)
                    str += origin.Substring(0, position - 2);
                else
                    str += origin.Substring(0, position - 1);


                if (unicode != 0)
                    str += (unicode).ToString();
                if (hangul.flag_space)
                {
                    str += " ";
                    hangul.flag_space = false;
                    spaceflag = true;
                }

                if (doubleflag)
                {
                    if (impossiblejongsungflag)
                        str += hangul.jongsung;
                    else
                        str += hangul.jongsung2;
                }
                if (hangul.jungsung.Equals("·"))
                {
                    str += "·";
                    dotflag = true;
                }
                else if (hangul.jungsung.Equals("‥"))
                {
                    str += "‥";
                    dotflag = true;
                }

                str += origin.Substring(position);
                et.Text = str;

                if (dotflag)
                    position++;
                if (doubleflag)
                {
                    if (!hangul.flag_doubled)
                        position++;
                    hangul.flag_doubled = true;
                }
                else
                {
                    if (hangul.flag_doubled)
                        position--;
                    hangul.flag_doubled = false;
                }
                if (spaceflag)
                    position++;
                if (unicode == 0 && dotflag == false)
                    position--;
                if (hangul.flag_addcursor)
                {
                    hangul.flag_addcursor = false;
                    position++;
                }

                if (hangul.flag_dotused)
                {
                    if (hangul.chosung.Length == 0 && dotflag == false)
                        et.SelectionStart = position;
                    else
                        et.SelectionStart = position - 1;
                }
                else if (!hangul.flag_writing && dotflag == false)
                    et.SelectionStart = (position + 1);
                else
                    et.SelectionStart = (position);

                hangul.flag_dotused = false;
                hangul.flag_writing = (unicode == 0 && dotflag == false) ? false : true;
            }
            else //if(mode == ENGLISH || mode == UPPER_ENGLISH || mode == NUMBER)
            {
                if (flag_engdelete)
                    str += origin.Substring(0, position - 1);
                else
                    str += origin.Substring(0, position);

                if (flag_upper || mode == NUMBER)
                    str += engnum;
                else
                    str += engnum.ToLower();

                if (flag_engdelete)
                {
                    str += origin.Substring(position);
                    et.Text = (str);
                    et.SelectionStart = (position);
                    flag_engdelete = false;
                }
                else
                {
                    str += origin.Substring(position);
                    et.Text = (str);
                    if (engnum.Length == 0)
                        et.SelectionStart = (position);
                    else
                        et.SelectionStart = (position + 1);
                }

                if (flag_initengnum)
                    init_engnum();
            }
        }
       
    }
}
