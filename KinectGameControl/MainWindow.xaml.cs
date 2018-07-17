using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Linq;
using System.Runtime.InteropServices;

namespace KinectGameControl
{
    public partial class MainWindow : Window
    {
        //声明KinectID
        private KinectSensor sensor = KinectSensor.KinectSensors[0];
        private readonly Brush[] skeletonBrushes;//绘图笔刷
        private Skeleton[] frameSkeletons;

        bool Jlast = false;
        bool Klast = false;
        bool Wlast = false;
        bool Alast = false;
        bool Slast = false;
        bool Blast = false;
        bool Dlast = false;
        bool Ulast = false;
        bool Ilast = false;
        bool Olast = false;
        bool Llast = false;
        float SpineX = 0;
        float SpineY = 0;
        int flag;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

            //平滑处理
            TransformSmoothParameters parameters = new TransformSmoothParameters();
            parameters.Smoothing = 0.7f;
            parameters.Correction = 0.3f;
            parameters.Prediction = 0.4f;
            parameters.JitterRadius = 1.0f;
            parameters.MaxDeviationRadius = 0.5f;
            sensor.SkeletonStream.Enable(parameters);

            //开启骨架功能
            sensor.SkeletonStream.Enable();
            //开启颜色功能
            sensor.ColorStream.Enable();

            skeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.sensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        void RadioButton1_Checked(object sender,EventArgs e)
        {
            //Console.WriteLine(Radio1.Content);
            flag = 1;
            Jlast = false;
            Klast = false;
            Wlast = false;
            Alast = false;
            Slast = false;
            Blast = false;
            Dlast = false;
        }
        void RadioButton2_Checked(object sender, EventArgs e)
        {
            // Console.WriteLine(Radio2.Content);
            flag = 2;
            bool Jlast = false;
            bool Klast = false;
            bool Wlast = false;
            bool Alast = false;
            bool Slast = false;
            bool Blast = false;
            bool Dlast = false;
            bool Ulast = false;
            bool Ilast = false;
            bool Olast = false;
            bool Llast = false;
            float SpineX = 0;
            float SpineY = 0;
        }

        //窗口开启，安装设备
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //声明骨架追踪处理函数
            sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            //声明颜色追踪处理函数
            sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);

            sensor.Start();
        }

        //窗口关闭，卸载设备
        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            sensor.Stop();
        }

        //骨架追踪
        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (SkeletonFrame SFrame = e.OpenSkeletonFrame())
            {
                if (SFrame == null)
                {
                    Console.WriteLine("Skeleton Frame null");
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    //Console.WriteLine("Skeleton Frame Exist");
                    frameSkeletons = new Skeleton[SFrame.SkeletonArrayLength];
                    SFrame.CopySkeletonDataTo(frameSkeletons);
                    receivedData = true;
                    Polyline figure;
                    Brush userBrush;
                    Skeleton skeleton;
                    LayoutRoot.Children.Clear();
                    SFrame.CopySkeletonDataTo(this.frameSkeletons);


                    for (int i = 0; i < this.frameSkeletons.Length; i++)
                    {
                        skeleton = this.frameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length];

                            //绘制头和躯干
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter,
                                                                                JointType.ShoulderLeft, JointType.Spine,
                                                                                JointType.ShoulderRight, JointType.ShoulderCenter,
                                                                                JointType.HipCenter});
                            LayoutRoot.Children.Add(figure);

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制作腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft,
                                                        JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight,
                                                        JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制左臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft,
                                                                                JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight,
                                                                                JointType.WristRight, JointType.HandRight });
                            LayoutRoot.Children.Add(figure);
                        }
                    }
                }
            }

            Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
            {
                Polyline figure = new Polyline();

                figure.StrokeThickness = 8;
                figure.Stroke = brush;

                for (int i = 0; i < joints.Length; i++)
                {
                    figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
                }

                return figure;
            }

            Point GetJointPoint(Joint joint)
            {
                ColorImagePoint point = this.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, this.sensor.ColorStream.Format);
                point.X *= (int)this.LayoutRoot.ActualWidth / sensor.DepthStream.FrameWidth;
                point.Y *= (int)this.LayoutRoot.ActualHeight / sensor.DepthStream.FrameHeight;

                return new Point(point.X, point.Y);
            }

            if (receivedData)
            {
                Skeleton currentSkeleton = (from s in frameSkeletons
                                            where s.TrackingState == SkeletonTrackingState.Tracked
                                            select s).FirstOrDefault();

                if (currentSkeleton != null)
                {
                    //Console.WriteLine("检测到骨架");
                    //获取关节点
                    Joint jointRight = currentSkeleton.Joints[JointType.HandRight];
                    Joint jointLeft = currentSkeleton.Joints[JointType.HandLeft];
                    Joint jointElbowRight = currentSkeleton.Joints[JointType.ElbowRight];
                    Joint jointElbowLeft = currentSkeleton.Joints[JointType.ElbowLeft];
                    Joint jointShoulderRight = currentSkeleton.Joints[JointType.ShoulderRight];
                    Joint jointShoulderLeft = currentSkeleton.Joints[JointType.ShoulderLeft];
                    Joint jointHead = currentSkeleton.Joints[JointType.Head];
                    Joint jointSpine = currentSkeleton.Joints[JointType.Spine];
                    Joint jointFootRight = currentSkeleton.Joints[JointType.FootRight];
                    Joint jointFootLeft = currentSkeleton.Joints[JointType.FootLeft];

                    //控制键盘---多次点击
                    //if (jointRight.Position.Y > 0.7)
                    //{
                    //    System.Windows.Forms.SendKeys.SendWait("b");
                    //}

                    //控制键盘---持续点击
                    if (flag == 1)
                    {
                        bool Know = jointLeft.Position.Z - jointShoulderLeft.Position.Z < -0.45;
                        if (Know && !Klast)
                        {
                            keybd_event(75, 0, 0, 0);
                        }
                        if (!Know && Klast)
                        {
                            keybd_event(75, 0, 2, 0);
                        }
                        Klast = Know;

                        bool Bnow = jointRight.Position.Y > 0.7;
                        if (Bnow && !Blast)
                        {
                            keybd_event(66, 0, 0, 0);
                        }
                        if (!Bnow && Blast)
                        {
                            keybd_event(66, 0, 2, 0);
                        }
                        Blast = Bnow;

                        bool Jnow = jointRight.Position.Z - jointShoulderRight.Position.Z < -0.45;
                        if (Jnow && !Jlast)
                        {
                            keybd_event(74, 0, 0, 0);
                        }
                        if (!Jnow && Jlast)
                        {
                            keybd_event(74, 0, 2, 0);
                        }
                        Jlast = Jnow;


                        bool Wnow = jointLeft.Position.Y > 0.5;
                        if (Wnow && !Wlast)
                        {
                            keybd_event(87, 0, 0, 0);
                        }
                        if (!Wnow && Wlast)
                        {
                            keybd_event(87, 0, 2, 0);
                        }
                        Wlast = Wnow;


                        bool Snow = jointLeft.Position.Y < 0;
                        if (Snow && !Slast)
                        {
                            keybd_event(83, 0, 0, 0);
                        }
                        if (!Snow && Slast)
                        {
                            keybd_event(83, 0, 2, 0);
                        }
                        Slast = Snow;


                        bool Dnow = jointLeft.Position.X - jointSpine.Position.X > -0.2;
                        if (Dnow && !Dlast)
                        {
                            keybd_event(68, 0, 0, 0);
                        }
                        if (!Dnow && Dlast)
                        {
                            keybd_event(68, 0, 2, 0);
                        }
                        Dlast = Dnow;


                        bool Anow = jointLeft.Position.X - jointSpine.Position.X < -0.5;
                        if (Anow && !Alast)
                        {
                            keybd_event(65, 0, 0, 0);
                        }
                        if (!Anow && Alast)
                        {
                            keybd_event(65, 0, 2, 0);
                        }
                        Alast = Anow;
                    }
                    else if (flag == 2)
                    {
                        bool Onow = (jointLeft.Position.Y > 0.7) && (jointRight.Position.Y > 0.7);
                        if (Onow && !Olast)
                        {
                            keybd_event(79, 0, 0, 0);
                        }
                        if (!Onow && Olast)
                        {
                            keybd_event(79, 0, 2, 0);
                        }
                        Olast = Onow;

                        bool Unow = jointLeft.Position.X - jointSpine.Position.X < -0.5;
                        if (Unow && !Ulast)
                        {
                            keybd_event(85, 0, 0, 0);
                        }
                        if (!Unow && Ulast)
                        {
                            keybd_event(85, 0, 2, 0);
                        }
                        Ulast = Unow;

                        bool Inow = jointFootLeft.Position.Z - jointSpine.Position.Z < -0.6;
                        if (Inow && !Ilast)
                        {
                            keybd_event(73, 0, 0, 0);
                        }
                        if (!Inow && Ilast)
                        {
                            keybd_event(73, 0, 2, 0);
                        }
                        Ilast = Inow;

                        bool Know = jointFootRight.Position.Z - jointSpine.Position.Z < -0.6;
                        if (Know && !Klast)
                        {
                            keybd_event(75, 0, 0, 0);
                        }
                        if (!Know && Klast)
                        {
                            keybd_event(75, 0, 2, 0);
                        }
                        Klast = Know;

                        bool Jnow = jointRight.Position.X - jointSpine.Position.X > 0.5;
                        if (Jnow && !Jlast)
                        {
                            keybd_event(74, 0, 0, 0);
                        }
                        if (!Jnow && Jlast)
                        {
                            keybd_event(74, 0, 2, 0);
                        }
                        Jlast = Jnow;


                        bool Wnow = ((jointSpine.Position.Y - SpineY) * 100 > 1) && (jointSpine.Position.Y > 0);
                        if (Wnow && !Wlast)
                        {
                            keybd_event(87, 0, 0, 0);
                        }
                        if (!Wnow && Wlast)
                        {
                            keybd_event(87, 0, 2, 0);
                        }
                        Wlast = Wnow;

                        bool Snow = ((jointSpine.Position.Y - SpineY) * 100 < -1);
                        if (Snow && !Slast)
                        {
                            keybd_event(83, 0, 0, 0);
                        }
                        if (!Snow && Slast)
                        {
                            keybd_event(83, 0, 2, 0);
                        }
                        Slast = Snow;
                        SpineY = jointSpine.Position.Y;

                        bool Dnow = (jointSpine.Position.X - SpineX) * 100 > 1;
                        if (Dnow && !Dlast)
                        {
                            keybd_event(68, 0, 0, 0);
                        }
                        if (!Dnow && Dlast)
                        {
                            keybd_event(68, 0, 2, 0);
                        }
                        Dlast = Dnow;


                        bool Anow = (jointSpine.Position.X - SpineX) * 100 < -1;
                        if (Anow && !Alast)
                        {
                            keybd_event(65, 0, 0, 0);
                        }
                        if (!Anow && Alast)
                        {
                            keybd_event(65, 0, 2, 0);
                        }
                        Alast = Anow;
                        SpineX = jointSpine.Position.X;
                    }
                }
                else
                {
                    //Console.WriteLine("未检测到骨架");
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(
            byte bVk,    //虚拟键值
            byte bScan,// 一般为0
            int dwFlags,  //这里是整数类型  0 为按下，2为释放
            int dwExtraInfo  //这里是整数类型 一般情况下设成为 0
        );
        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,PixelFormats.Bgr32, 
                                                                    null, pixelData,frame.Width * frame.BytesPerPixel);
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.sensor = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.sensor = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }
    }
}

