using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO.Ports;



namespace SerialProtocol
{
    
    class SerialProtocol
    {

        protected static byte CMD_READ_PARAMS = Convert.ToByte('R');
        protected static byte CMD_WRITE_PARAMS = Convert.ToByte('W');
        protected static byte CMD_REALTIME_DATA = Convert.ToByte('D');
        protected static byte CMD_BOARD_INFO = Convert.ToByte('V');
        protected static byte CMD_CALIB_ACC = Convert.ToByte('A');
        protected static byte CMD_CALIB_GYRO = Convert.ToByte('g');
        protected static byte CMD_CALIB_EXT_GAIN = Convert.ToByte('G');
        protected static byte CMD_USE_DEFAULTS = Convert.ToByte('F');
        protected static byte CMD_CALIB_POLES = Convert.ToByte('P');
        protected static byte CMD_RESET = Convert.ToByte('r');
        protected static byte CMD_HELPER_DATA = Convert.ToByte('H');
        protected static byte CMD_CALIB_OFFSET = Convert.ToByte('O');
        protected static byte CMD_CALIB_BAT = Convert.ToByte('B');
        protected static byte CMD_MOTORS_ON = Convert.ToByte('M');
        protected static byte CMD_MOTORS_OFF = Convert.ToByte('m');
        protected static byte CMD_CONTROL = Convert.ToByte('C');
        protected static byte CMD_TRIGGER_PIN = Convert.ToByte('T');
        protected static byte CMD_EXECUTE_MENU = Convert.ToByte('E');
        protected static byte CMD_GET_ANGLES = Convert.ToByte('I');
        protected static byte CMD_CONFIRM = Convert.ToByte('C');
        
        protected static byte CMD_BOARD_INFO_3 = 20;
        protected static byte CMD_READ_PARAMS_3 = 21;
        protected static byte CMD_WRITE_PARAMS_3 = 22;
        protected static byte CMD_REALTIME_DATA_3 = 23;
        protected static byte CMD_SELECT_IMU_3 = 24;
        protected static byte CMD_ERROR = (byte)255;
        protected static byte MAGIC_BYTE = Convert.ToByte('>');
        protected static bool BOARD_VERSION_3 = false;
        protected static float ANGLE_TO_DEGREE = 0.02197266F;

        public static int MODE_NO_CONTROL = 0;
        public static int MODE_SPEED = 1;
        public static int MODE_ANGLE = 2;
        public static int MODE_SPEED_ANGLE = 3;
        public static int MODE_RC = 4;

        // fixed data[] positions
        protected static int MAGIC_BYTE_POS = 0;
        protected static int COMMAND_ID_POS = 1;
        protected static int DATA_SIZE_POS = 2;
        protected static int HEADER_CHECKSUM_POS = 3;
        protected static int BODY_DATA_POS = 4;

        private static SerialPort port = new SerialPort("COM4",115200);
        private byte[] byteArray = {62, 67, 13, 80, 2, 85, 5, 85, 5, 85, 5, 85, 5, 85, 5, 85, 5, 30};

        static void Main(string[] args)
        {
            SerialProtocol p = new SerialProtocol();
            ControlCommandStructure cCmd = new ControlCommandStructure();

            cCmd.setMode(MODE_ANGLE);
            cCmd.setAnglePitch(30);
            cCmd.setAngleRoll(30);
            cCmd.setAngleYaw(30);

            cCmd.setSpeedPitch(30);
            cCmd.setSpeedRoll(30);
            cCmd.setSpeedYaw(30);


            byte[] byteRead = new byte[100];
            
            //port.Write(p.byteArray, 0, 18);
            

            
            if (sendCommand(CMD_CONTROL,cCmd.getControlStructure()))
            {
                Console.WriteLine("send a message sucessfully");
                port.Read(byteRead, 0, 100);
            }

            else
            {
                Console.WriteLine("Can't send command a message");
                return;
            }
            
        }

        public byte[] getByteArray()
        {
            return byteArray;
        }

   

        private SerialProtocol()
        {            
            // Begin communications
            port.Open();            
        }

        protected static bool verifyChecksum(byte[] data)
        {
            if (data.Length <= 4)
                return false;

            bool headerOK = false;
            bool bodyOK = false;

            if (data[MAGIC_BYTE_POS] == MAGIC_BYTE
                    && ((int)(0xff & data[COMMAND_ID_POS]) + (int)(0xff & data[DATA_SIZE_POS])) % 256 == (0xff & data[HEADER_CHECKSUM_POS]))
            {
                headerOK = true;
            }
            else {
                Console.WriteLine("Bad Header");
                return false;
            }

            int bodyChksm = 0;
            for (int i = 4; i < data.Length - 1; i++)
            {
                bodyChksm += (0xff & data[i]);
            }

            if ((bodyChksm % 256) == (0xff & data[data.Length - 1]))
            {
                bodyOK = true;
            }
            else {
                Console.WriteLine("Bad Body");
                return false;
            }

            return (headerOK && bodyOK);
        }

        public static bool sendCommand(byte commandID, byte[] rawData)
        {
            byte bodyDataSize = (byte)rawData.Length;
            byte headerChecksum = (byte)(((int)commandID + (int)bodyDataSize) % 256);
            int rawBodyChecksum = 0;
            int cnt = 0;
            do
            {
                if (cnt >= bodyDataSize)
                {
                    byte bodyChecksum = (byte)(rawBodyChecksum % 256);
                    byte[] headerArray = new byte[4];
                    headerArray[MAGIC_BYTE_POS] = MAGIC_BYTE;
                    headerArray[COMMAND_ID_POS] = (byte)(commandID & 0xff);
                    headerArray[DATA_SIZE_POS] = (byte)(bodyDataSize & 0xff);
                    headerArray[HEADER_CHECKSUM_POS] = (byte)(headerChecksum & 0xff);

                    byte[] headerAndBodyArray = new byte[1 + (headerArray.Length + rawData.Length)];
                    Array.Copy(headerArray, headerAndBodyArray, headerArray.Length);
                    Array.Copy(rawData,0,headerAndBodyArray,headerArray.Length,rawData.Length);
                    
                    headerAndBodyArray[headerArray.Length + rawData.Length] = (byte)(bodyChecksum & 0xff);

                    if (verifyChecksum(headerAndBodyArray))
                    {
                        /*
                        foreach(byte item in headerAndBodyArray)
                        {
                            Console.WriteLine(item);
                        }*/

                        port.Write(headerAndBodyArray,0,headerAndBodyArray.Length);        
                        return true;
                    }
                    else return false;
                    
                    
                }
                rawBodyChecksum += rawData[cnt];
                cnt++;
            } while (true);
        }

        //Basic wrapper function without bodydata
        public static void sendCommand(byte commandID)
        {
            sendCommand(commandID, new byte[0]);
        }
    }


    class ControlCommandStructure
    {

        public static int MODE_NO_CONTROL = 0;
        public static int MODE_SPEED = 1;
        public static int MODE_ANGLE = 2;
        public static int MODE_SPEED_ANGLE = 3;
        public static int MODE_RC = 4;

        private static float ANGLE_TO_DEGREE = 0.02197266F;
        private static int mode = 0;
        private static int speedRoll = 0;
        private static int angleRoll = 0;
        private static int speedPitch = 0;
        private static int anglePitch = 0;
        private static int speedYaw = 0;
        private static int angleYaw = 0;
        private static byte[] controlData = new byte[13];

        public byte[] getControlStructure()
        {
            return getCmdControlDataArray();
        }

        private static byte getFirstByte(int i)
        {
            return (byte)(i & 0xff);
        }

        private static byte getSecondByte(int i)
        {
            return (byte)(0xff & i >> 8);
        }

        private static int Degree2Angle(int i)
        {
            int x = (int)(i * (1.0f / ANGLE_TO_DEGREE));

            return x;
        }

        public static int getIntSigned(byte byte0, byte byte1)
        {
            return (byte0 & 0xff) + (byte1 << 8);
        }

        public static byte[] getCmdControlDataArray()
        {
            controlData[0] = (byte)(0xff & mode);
            if (mode == MODE_ANGLE || mode == MODE_SPEED)
            {
                controlData[1] = getFirstByte(Degree2Angle(speedRoll));
                controlData[2] = getSecondByte(Degree2Angle(speedRoll));
                controlData[3] = getFirstByte(Degree2Angle(angleRoll));
                controlData[4] = getSecondByte(Degree2Angle(angleRoll));
                controlData[5] = getFirstByte(Degree2Angle(speedPitch));
                controlData[6] = getSecondByte(Degree2Angle(speedPitch));
                controlData[7] = getFirstByte(Degree2Angle(anglePitch));
                controlData[8] = getSecondByte(Degree2Angle(anglePitch));
                controlData[9] = getFirstByte(Degree2Angle(speedYaw));
                controlData[10] = getSecondByte(Degree2Angle(speedYaw));
                controlData[11] = getFirstByte(Degree2Angle(angleYaw));
                controlData[12] = getSecondByte(Degree2Angle(angleYaw));
            }
            else if (mode == MODE_RC)
            {

                controlData[1] = getFirstByte(speedRoll);
                controlData[2] = getSecondByte(speedRoll);
                controlData[3] = getFirstByte(angleRoll);
                controlData[4] = getSecondByte(angleRoll);
                controlData[5] = getFirstByte(speedPitch);
                controlData[6] = getSecondByte(speedPitch);
                controlData[7] = getFirstByte(anglePitch);
                controlData[8] = getSecondByte(anglePitch);
                controlData[9] = getFirstByte(speedYaw);
                controlData[10] = getSecondByte(speedYaw);
                controlData[11] = getFirstByte(angleYaw);
                controlData[12] = getSecondByte(angleYaw);

            }

            return controlData;
        }

        public static int getMode()
        {
            return mode;
        }

        public void setMode(int m)
        {
            mode = m;
        }

        public int getSpeedRoll()
        {
            return speedRoll;
        }

        public void setSpeedRoll(int s)
        {
            speedRoll = s;
        }

        public int getAngleRoll()
        {
            return angleRoll;
        }

        public void setAngleRoll(int a)
        {
            angleRoll = a;
        }

        public int getSpeedPitch()
        {
            return speedPitch;
        }

        public void setSpeedPitch(int s)
        {
            speedPitch = s;
        }

        public int getAnglePitch()
        {
            return anglePitch;
        }

        public void setAnglePitch(int a)
        {
            anglePitch = a;
        }

        public int getSpeedYaw()
        {
            return speedYaw;
        }

        public void setSpeedYaw(int s)
        {
            speedYaw = s;
        }

        public int getAngleYaw()
        {
            return angleYaw;
        }

        public void setAngleYaw(int a)
        {
            angleYaw = a;
        }
    }

}
