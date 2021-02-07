using NLog;

using System;

using System.Collections.Generic;

using System.Configuration;

using System.Data;

namespace HR_WorkTime_Info

{

    class Program

    {

        static void Main(string[] args)

        {

            LogManager.GetCurrentClassLogger().Info($"근무정보 전송 시작");

            SyncTransferWorkInfo();

            LogManager.GetCurrentClassLogger().Info($"근무정보 전송 종료{Environment.NewLine}");

        }







        static void SyncTransferWorkInfo()

        {



            try

            {



                #region Step 1 : WorkTime 근무 정보 조회   

                //--> worktime 조회 select 

                List<object> items = new List<object>();

                //oracle DB connection 

                DBHelper_oracle dbHelper_oracle = new DBHelper_oracle();

                DBHelper dbHelper = new DBHelper();

                //select 

                string sql = @"

 SELECT	 U.user_id AS user_id

		 ,WT.work_date AS work_date 

         ,WT.start_time AS start_time

		 ,WT.end_time AS end_time

         ,WT.computer_no

		 ,ISNULL((SELECT work_start_time FROM [dbo].[tb_worktime_sync] WS WHERE WS.work_date = CONVERT(DATE, GETDATE()) AND WS.user_no = U.user_no), '00:00:00') AS sync_start_time

		 ,ISNULL((SELECT work_end_time FROM [dbo].[tb_worktime_sync] WS WHERE WS.work_date = CONVERT(DATE, GETDATE()) AND WS.user_no = U.user_no), '00:00:00') AS sync_end_time

         ,ROUND(WT.work_time/60,0) AS work_time  

         ,ROUND(WT.over_time/60,0) AS over_time

         ,last_access_time

          FROM [dbo].[tb_work_time] WT WITH(NOLOCK) 

          INNER JOIN [dbo].[tb_user] U WITH(NOLOCK)

          ON U.user_no = WT.user_no

          INNER JOIN [dbo].[tb_company] C WITH(NOLOCK)

          ON (U.company_no = C.company_no AND U.retireYN = 'N')

          WHERE WT.work_date = CONVERT(VARCHAR, GETDATE()-1, 112)";

                // 퇴근시간은 LAST ACCESS 시간 기준으로 전달하며 PC를 켜놓고 간 사람의 경우 ACCESS 시간이 아닌 스케쥴 종료시간으로 전달.

                // last_access_time + 

                //--> 켜놓고 간사람 == 





                string connectionString_HR = ConfigurationManager.ConnectionStrings["ConnDB_HR"].ConnectionString;

                string connectionString = ConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString;

                #endregion



                using (DataTable dt = dbHelper.ExecuteSQL(sql, connectionString))

                {

                    if (dt == null || dt.Rows.Count <= 0)

                    {

                        LogManager.GetCurrentClassLogger().Info($"WorkTime DB 근무정보 데이터 없음");

                        return;

                    }



                    #region Step 2 : SEWC ERP에 근무데이터 추가

                    int sync_count = 0;

                    List<string> tk_work_time_no_list = new List<string>();

                    foreach (DataRow dr in dt.Rows)

                    {

                        //--> Log (DataTable log) 

                        //Data Log  

                        LogManager.GetCurrentClassLogger().Info($"{dr["user_id"]}");

                        LogManager.GetCurrentClassLogger().Info($"{((DateTime)dr["work_date"]).ToString("yyyyMMdd")}");

                        LogManager.GetCurrentClassLogger().Info($"{new DateTime(((TimeSpan)dr["start_time"]).Ticks).ToString("HHmm")}");

                        LogManager.GetCurrentClassLogger().Info($"{new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HHmm")}");

                        LogManager.GetCurrentClassLogger().Info($"{new DateTime(((TimeSpan)dr["sync_start_time"]).Ticks).ToString("HHmm")}");

                        LogManager.GetCurrentClassLogger().Info($"{new DateTime(((TimeSpan)dr["sync_end_time"]).Ticks).ToString("HHmm")}");

                        LogManager.GetCurrentClassLogger().Info($"{Convert.ToInt32(dr["work_time"])}");

                        LogManager.GetCurrentClassLogger().Info($"{Convert.ToInt32(dr["over_time"])}");

                        LogManager.GetCurrentClassLogger().Info($"{Convert.ToString(dr["computer_no"])}");

                        LogManager.GetCurrentClassLogger().Info($"{Convert.ToString(dr["last_access_time"])}");



                        string user_id = Convert.ToString(dr["user_id"]);

                        string work_date = ((DateTime)dr["work_date"]).ToString("yyyyMMdd");

                        string start_time = new DateTime(((TimeSpan)dr["start_time"]).Ticks).ToString("HHmm");



                        string sync_start_time = new DateTime(((TimeSpan)dr["sync_start_time"]).Ticks).ToString("HHmm");

                        string sync_end_time = new DateTime(((TimeSpan)dr["sync_end_time"]).Ticks).ToString("HHmm");









                        /* Pet 은 Parse_end_time.ToString("HHmm") 입니다.   */

                        //숭의 여대 측에서 소정근무한 시간값은 크게 중요하지 않다고 하여 실제 수치가 아닌 0 으로 보내주시면 됩니다.

                        //* HWORK 의 값은 0으로 전달 

                        // --> PCOFF.HIN221M 의 HWORK 의 값은 0 으로 전달해주세요 라는 요구사항 <--

                        // 2021-02-03 요구 사항 변경으로 인한 모든값에 대하여 0으로 고정

                        //int work_time = Convert.ToInt32(dr["work_time"]);

                        //int work_time == 실제 수치( 실 근무시간 ) 

                        int work_time = 0;

                        // 형변환 이슈 존재 고칠것

                        int over_time = Convert.ToInt32(dr["over_time"]);

                        string computer_no = Convert.ToString(dr["computer_no"]);

                        LogManager.GetCurrentClassLogger().Info("hello1");  //test 

                        

                        string end_time = new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HH:mm");

                        LogManager.GetCurrentClassLogger().Info("hello2");  //test 

                        string last_access_time  = new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HH:mm");

                        LogManager.GetCurrentClassLogger().Info("hello3");  //test 



                        //string last_access_time = new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HHmm"); 

                        //DateTime 으로 Parse  

                        LogManager.GetCurrentClassLogger().Info("hello--1");    //test 



                        string DtString1 = end_time;

                        LogManager.GetCurrentClassLogger().Info("hello--2");    //test 

                        string DtString2 = last_access_time;

                        LogManager.GetCurrentClassLogger().Info("hello--3");    //test 



                        //end_time = 1800

                        LogManager.GetCurrentClassLogger().Info("hello--3-1    end_time : " + end_time);    //test 

                        DateTime Parse_end_time = DateTime.Parse(end_time);

                        LogManager.GetCurrentClassLogger().Info("hello--4");    //test 

                        LogManager.GetCurrentClassLogger().Info("hello--3-2   last_access_time  : " + last_access_time);

                        DateTime Parse_last_access_time = DateTime.Parse(last_access_time);

                        LogManager.GetCurrentClassLogger().Info("hello--5");    //test 

                        //Agent가 서버와 마지막으로 통신한 시간 또는 마지막으로 통신한 시간이 숭의여대에서 제공해주는 VW_WORK_PCOFF 테이블에서 규정퇴근시간

                        // 보다 클 경우 규정퇴근시간으로 값을 insert 해달라는 요청사항 입니다.



                        //2021-02-23 22:30 -- 1번 시도  

                        //마지막으로 통신한 시간(Parse_last_access_time) 이 규정퇴근시간(Parse_end_time) 보다 큰경우 에는

                        // 규정 퇴근시간 (Parse_end_time)으로 값을 insert 해야한다

                        //퇴근시간은 LAST ACCESS 시간 기준으로 전달하며 PC를 켜놓고 간 사람의 경우 ACCESS 시간이 아닌 스케쥴 종료시간으로 전달.	





                        string Pet= Parse_end_time.ToString("HHmm");

                        LogManager.GetCurrentClassLogger().Info("hello4");  //test 



                        if (Parse_end_time < Parse_last_access_time)

                        {

                            LogManager.GetCurrentClassLogger().Info("hello5");  //test 

                            Pet = Parse_last_access_time.ToString("HHmm");

                            

                        }

                        LogManager.GetCurrentClassLogger().Info("hello6");  //test 



                        //요구사항2)

                        //하루단위로 끊어서 그날 사용한 연장근무의 값만 보내주세요

                        /*

                        해당 내용은 현재 연장근무시간에 대하여 누계값



                        EX) 2월 1일에 연장근무 2시간 2월 2일에 연장근무 2시간을 하는 경우



                        2월 1일의 HOVERTIME_MM은 2시간 2월 2일의 HOVERTIME_MM은 4시간으로 보내고 있는 상황입니다.







                        해당 내용을 하루단위로 끊어서 그날 사용한 연장근무의 값만 보내달라고 하는 요청사항입니다.

                        */

                        //==> 누계 값을 전달하는것이 아니라 하루단위로 끊어서 그날 사용한 연장근무 의 시간 만 보내달라는 상황 







                        //                        sql = $@"

                        //DECLARE 

                        //	v_user_id  HIN221M.USER_ID%TYPE;

                        //	v_work_date HIN221M.WORK_DATE%TYPE;

                        //BEGIN

                        //    BEGIN

                        //	SELECT 	USER_ID, WORK_DATE 

                        //    INTO    v_user_id, v_work_date

                        //	FROM 	HIN221M 

                        //	WHERE 	USER_ID = '{dr["user_id"]}' AND  work_date ='{((DateTime)dr["work_date"]).ToString("yyyyMMdd")}';



                        //    UPDATE HIN221M

                        //              SET ST_ATTEND_TIME = '{new DateTime(((TimeSpan)dr["start_time"]).Ticks).ToString("HHmm")}'

                        //                  ,ST_LEAVE_TIME = '{new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HHmm")}'

                        //                  ,START_TIME = '{new DateTime(((TimeSpan)dr["sync_start_time"]).Ticks).ToString("HHmm")}'

                        //                  ,END_TIME = '{new DateTime(((TimeSpan)dr["sync_end_time"]).Ticks).ToString("HHmm")}'

                        //              WHERE USER_ID = '{dr["user_id"]}' 

                        //	            AND work_date = '{((DateTime)dr["work_date"]).ToString("yyyyMMdd")}';



                        //            EXCEPTION

                        //            WHEN NO_DATA_FOUND THEN

                        //            v_user_id := NULL;

                        //            v_work_date := NULL;





                        //                IF v_user_id IS NULL AND v_work_date IS NULL THEN

                        //                  INSERT INTO HIN221M

                        //                    (USER_ID, WORK_DATE, ST_ATTEND_TIME, ST_LEAVE_TIME, START_TIME, END_TIME,HWORK,HOVERTIME_MM)

                        //                  VALUES

                        //                    (

                        //                         '{dr["user_id"]}'

                        //                        , '{((DateTime)dr["work_date"]).ToString("yyyyMMdd")}'

                        //                        , '{new DateTime(((TimeSpan)dr["start_time"]).Ticks).ToString("HHmm")}'

                        //                        , '{new DateTime(((TimeSpan)dr["end_time"]).Ticks).ToString("HHmm")}'

                        //                        , '{new DateTime(((TimeSpan)dr["sync_start_time"]).Ticks).ToString("HHmm")}'

                        //                        , '{new DateTime(((TimeSpan)dr["sync_end_time"]).Ticks).ToString("HHmm")}'

                        //                        ,  {Convert.ToInt32(dr["work_time"])}

                        //                        ,  {Convert.ToInt32(dr["over_time"])}

                        //                   );

                        //                END IF;

                        //  END;

                        //END;

                        //        ";



                        // 숭의여대 Oracle DataBase MERGE 

                        //HWORK 를 0으로 보내 주시면 됩니다.



                        LogManager.GetCurrentClassLogger().Info("hello7");  //test 

                        sql = $@"

                        MERGE INTO 

                            HIN221M_TEST

                        USING DUAL 

                            ON (USER_ID = '{user_id}'  AND WORK_DATE = '{work_date}' AND  COMPUTER_NO = '{computer_no}') 

                            WHEN MATCHED THEN     

                                UPDATE SET     

                                        ST_ATTEND_TIME = '{start_time}',

                                        ST_LEAVE_TIME = '{Pet}',

                                        START_TIME = '{sync_start_time}',

                                        END_TIME = '{sync_end_time}',

                                        HWORK = {work_time},

                                        HOVERTIME_MM = {over_time}

                            WHEN NOT MATCHED THEN

                                INSERT

                                (USER_ID, WORK_DATE, ST_ATTEND_TIME, ST_LEAVE_TIME, START_TIME, END_TIME,HWORK,HOVERTIME_MM,COMPUTER_NO)

                                VALUES

                                ('{user_id}', '{work_date}', '{start_time}', '{Pet}', '{sync_start_time}', '{sync_end_time}', {work_time}, {over_time},'{computer_no}')";

                        LogManager.GetCurrentClassLogger().Info("hello8");  //test 

                        NLog.LogManager.GetCurrentClassLogger().Trace($"{Environment.NewLine}{sql}");// modifier

                        LogManager.GetCurrentClassLogger().Info("hello9");  //test 

                        dbHelper_oracle.ExecuteSQL(sql, connectionString_HR);



                        LogManager.GetCurrentClassLogger().Info("hello10");  //test 

                    }





                    #endregion



                }

            }



            catch (Exception ex)

            {

                LogManager.GetCurrentClassLogger().Info($"StackTrace : {ex.StackTrace}");

                LogManager.GetCurrentClassLogger().Info($"Message : {ex.Message}");

            }

        }



    }

}
