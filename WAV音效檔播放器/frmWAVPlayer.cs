using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media; // 引用System.Media命名空間
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave; // 引用NAudio命名空間
using NReco.VideoConverter; // 引用NReco.VideoConverter命名空間
using NAudio.Lame; // 引用NAudio.Lame命名空間

namespace WAV音效檔播放器
{
    public partial class frmWAVPlayer : Form
    {
        SoundPlayer player; // 宣告SoundPlayer物件

        public frmWAVPlayer()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofdWAVFile.Filter = "WAV Files(*.wav)|*.wav";
            // 打開檔案對話方塊
            if (ofdWAVFile.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofdWAVFile.FileName;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            // 若沒有輸入
            if (string.IsNullOrWhiteSpace(txtPath.Text) || !File.Exists(txtPath.Text))
            {
                MessageBox.Show("請先選擇有效的 WAV 檔案！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (player != null) player.Stop(); // 先停止當前播放
                player = new SoundPlayer(); // 建立播放器物件
                player.SoundLocation = txtPath.Text; // 指定音效所在路徑檔名
                player.Load(); // 載入音效檔資料
                player.Play(); // 播放音效檔
            }
            catch (Exception ex)
            {
                MessageBox.Show("無法播放音效檔 : "+ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoop_Click(object sender, EventArgs e)
        {
            // 若沒有輸入
            if (string.IsNullOrWhiteSpace(txtPath.Text) || !File.Exists(txtPath.Text))
            {
                MessageBox.Show("請先選擇有效的 WAV 檔案！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (player != null) player.Stop(); // 先停止當前播放
                player = new SoundPlayer(txtPath.Text);
                player.PlayLooping();// 重複播放
            }
            catch (Exception ex)
            {
                MessageBox.Show("無法播放音效檔 : " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (player != null)
            {
                player.Stop();
            }
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmWAVPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("確定要關閉應用程式嗎？", "關閉確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // 取消關閉
            }
        }

        #region 轉檔功能
        private void btnConvert_Click(object sender, EventArgs e)
        {
            // 檢查是否有輸入有效的 WAV 檔案
            if (string.IsNullOrWhiteSpace(txtPath.Text) || !File.Exists(txtPath.Text))
            {
                MessageBox.Show("請先選擇要轉換的有效 WAV 檔案！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Path.GetExtension(txtPath.Text).ToLower() != ".wav")
            {
                MessageBox.Show("請確定選擇的是 WAV 檔案！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // MP3, AAC, WMA, FLAC
            sfdSave.Filter = "MP3 檔案 (*.mp3)|*.mp3|AAC 檔案 (*.m4a)|*.m4a|WMA 檔案 (*.wma)|*.wma|FLAC 無損音訊 (*.flac)|*.flac";
            sfdSave.FilterIndex = 1; // 預設為 MP3

            sfdSave.FileName = Path.GetFileNameWithoutExtension(txtPath.Text);

            if (sfdSave.ShowDialog() == DialogResult.OK)
            {
                // 暫停播放，釋放音效檔佔用
                if (player != null) player.Stop();

                this.Text = "WAV音效檔播放器 - 轉檔中請稍候...";
                Application.DoEvents(); // 強制刷新畫面

                try
                {
                    // NAudio 支援 MP3, AAC, WMA 格式轉換，FLAC 需要使用 NReco (底層為 FFmpeg) 處理
                    if (sfdSave.FilterIndex >= 1 && sfdSave.FilterIndex <= 3)
                    {
                        // 選項 1, 2, 3 用 NAudio
                        using (var reader = new AudioFileReader(txtPath.Text))
                        {
                            switch (sfdSave.FilterIndex)
                            {
                                case 1: // MP3
                                    using (var writer = new LameMP3FileWriter(sfdSave.FileName, reader.WaveFormat, 128))
                                    {
                                        reader.CopyTo(writer);
                                    }
                                    break;
                                case 2: // AAC
                                    MediaFoundationEncoder.EncodeToAac(reader, sfdSave.FileName);
                                    break;
                                case 3: // WMA
                                    MediaFoundationEncoder.EncodeToWma(reader, sfdSave.FileName);
                                    break;
                            }
                        }
                    }
                    else if (sfdSave.FilterIndex == 4)
                    {
                        // 選項 4 FLAC 用 NReco (底層為 FFmpeg) 處理
                        var ffMpeg = new FFMpegConverter();
                        ffMpeg.ConvertMedia(txtPath.Text, sfdSave.FileName, "flac");
                    }

                    MessageBox.Show("轉檔成功！\n檔案已儲存至：\n" + sfdSave.FileName, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("轉檔失敗 : " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // 復原標題文字
                    this.Text = "WAV音效檔播放器";
                }
            }
        }
        #endregion
    }
}