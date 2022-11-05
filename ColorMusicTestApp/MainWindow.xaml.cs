using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using System.IO.Ports;
using System.Numerics;
using Accord.Math;
using System.Threading;
using System.Text.RegularExpressions;

namespace ColorMusicTestApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPort sp = new SerialPort();
        string[] ports = SerialPort.GetPortNames();
        WasapiLoopbackCapture CaptureVolume = new WasapiLoopbackCapture();
        WasapiLoopbackCapture CaptureFrequency = new WasapiLoopbackCapture();
        private byte minValueVolume = 1;
        private byte minValueFreq = 100;

        public static double lastPeak = 0;


        public MainWindow()
        {
            InitializeComponent();
            COM.ItemsSource = ports;
            COM.SelectedIndex = 0;
        }

        public void COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sp.IsOpen)
                    sp.Close();
                sp.PortName = COM.SelectedItem as string;
                sp.BaudRate = 9600;
                sp.Open();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void CololrMusicVolumeStart()
        {
            CaptureVolume = new WasapiLoopbackCapture();
            CaptureVolume.DataAvailable += (s, e) =>
            {
                float max = 0f;
                var buffer = new WaveBuffer(e.Buffer);

                // интерпретировать как 32-битный звук с плавающей запятой
                for (int index = 0; index < e.BytesRecorded / 4; index++)
                {
                    var sample = buffer.FloatBuffer[index];
                    // Значение по модулю
                    if (sample < 0) sample = -sample;
                    // Устанавливаем новое макс. значение
                    if (sample > max) max = sample;
                }
                if (max.Equals(0))
                {
                    lastPeak = 0;
                }
                else
                {
                    byte[] array = new byte[1];
                    array[0] = (byte)TransformValue(max * 100);
                    if (array[0] < minValueVolume)
                        array[0] = 0;
                    sp.Write(array, 0, 1);
                }
            };
            CaptureVolume.RecordingStopped += (s, a) =>
            {
                CaptureVolume.Dispose();
            };
            CaptureVolume.StartRecording();

            double TransformValue(float peak)
            {
                lastPeak = Math.Floor(peak);
                return lastPeak;
            }
        }

        

        public void CololrMusicFrequencyStart()
        {
            CaptureFrequency = new WasapiLoopbackCapture();
            CaptureFrequency.DataAvailable += (s, e) =>
            {
                int Size = 2048;
                var buffer = new WaveBuffer(e.Buffer);
                Complex[] values = new System.Numerics.Complex[Size];
                for (int i = 0; i < values.Length; i++)
                    values[i] = new System.Numerics.Complex(buffer.FloatBuffer[i], 0.0);
                FourierTransform.FFT(values, FourierTransform.Direction.Forward);

                byte[] AmplitudeFreqValue = new byte[1];
                AmplitudeFreqValue[0] = (byte)((values[0].Magnitude * 100) + (values[1].Magnitude * 100) + (values[2].Magnitude * 100) + (values[3].Magnitude * 100));
                if (AmplitudeFreqValue[0] > minValueFreq)
                    AmplitudeFreqValue[0] = 0;
                sp.Write(AmplitudeFreqValue, 0, 1);
            };
            CaptureFrequency.RecordingStopped += (s, a) =>
            {
                CaptureFrequency.Dispose();
            };
            CaptureFrequency.StartRecording();
            Thread.Sleep(10);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == Volume)
                CololrMusicVolumeStart();
            else if (sender == Frequency)
                CololrMusicFrequencyStart();
            else if (sender == StopV)
                CaptureVolume.Dispose();
            else if (sender == StopF)
                CaptureFrequency.Dispose();
            
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == SliderV)
                minValueVolume = (byte)SliderV.Value;
            else
                minValueFreq = (byte)SliderF.Value;
        }
    }
}
