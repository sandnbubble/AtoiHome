using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using AtoiHomeServiceLib;

namespace AtoiHomeManager
{
    public partial class App : Application
    {
        // NotifyServer에서 사용하는 callback에서 UI로직을 처리하는것이 마음에 안들어서
        // App에서 처리하기 위해 추가됨
        // 이벤트 라우트 경로
        // TextTrasnferService.UploadImage -> Service Application -> NotifyServer -> Application.callback -> Application.OneClickShotEventHandler -> ViewModelEventHandler -> View
        //                                                                        |  
        //                                                                        |
        //                                                          Service side    Client side
        // 복잡해진 듯 하지만 경계는 확실하다.

        private void OneClickShotEventHandler(Object sender, OneClickShotEventArgs e)
        {
            try
            {
                //Samples.FancyBalloon balloon = new Samples.FancyBalloon();
                if (e.MessageType == MessageType.NOTIFYSERVICE_CLOSING)
                {
                    (Current as App).bConnected = false;
                    //balloon.BalloonText = "알림서버가 서비스를 중지했습니다";
                }
                else
                {
                    //IPC서버가 OneClickShot.UploadImage에서 발행한  이벤트에 포함된 업로드파일명을 라우트하여 전달
                    //balloon.BalloonText = e.Message + "이미지가 수신되었습니다";
                }
                //(Current as App).notifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 4000);

                if (Current.MainWindow == null)
                {
                    Current.MainWindow = new MainWindow();
                    Current.MainWindow.Show();
                }
                //View로 이벤트를 발행
                //구독자로 등록된 모든 View들은 이벤트를 받을 수 있음
                (Current as App).onModelContextEvent(this, new ModelContextArgs(e.MessageType, e.Message));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
