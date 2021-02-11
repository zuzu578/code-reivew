var g_ScanQRCode_callback; 
4 function ScanQRCode(callback_name){ 
5 	var param = 'cmd = ScanQRCode'; 
6 	param += '&cmd_type=NATIVE'; 
7 	g_ScanQRCode_callback = callback_name; 
8 	NM.ExecuteCommand(param); 
9 } 
10 // QR code 가 undefined 인 경우 
11 function ScanQRCodeCallback(qrcode){ 
12 	if(g_ScanQRCode_callback == undefined) 
13 		return; 
14 	window[g_ScanQRCode_callback](qrcode); 
15 
 
16 } 
17 
 
18 //스캔한 QR parameter 를  Handler 에 전달  
19 function ScanQRCodeCallback_SetScreenlock(qrcode){ 
20 
 
21 	var param = 'cmd=SetScreenlockQR'; 
22 	param += '&qrcode ='+encodeURIComponent(qrcode); 
23 	param += '&cmd_type = HTTP'; 
24 	param += '&callback = SetScreenlockQRCallback'; 
25 	NM.ExecuteCommand(param); 
26 } 
27 
 
28 
 
29 // Handler 부분  
30 
 
31 case "SetScreenlockQR": 
32 { 
33 	param.Add("screenlock_QR_no",Request.Params["qrcode"]); 
34 	param.Add("user_no",Request.Params["user_no"]); 
35 	param.Add("company_no",Request.Params["company_no"]); 
36 	Response.Write(new JavaScriptSerializer().Serialize(SetScreenlockQR(param))); 
37 	Response.End(); 
38 
 
39 } 
40 break; 
41 
 
42 
 
43 
 
44 // QR Parameter => DataBase  
45 public Dictionary<string , object> SetScreenlockQR(Dictionary<string , object> param) 
46 { 
47 
 
48 	Dictionary<string , object> result = null;  
49 	 
50 	try 
51 	{ 
52 	using (DataTable dt = dac.SetScreenlockQR(param)) 
53 	{ 
54 		if(dt == null || dt.Rows.Count <= 0) 
55 		{ 
56 			result = new Dictionary<string , object>(); 
57 			result.Add("result", "failure"); 
58 			result.Add("code","7"); 
59 			return result; 
60 			result = new Dictionary<string , object>(); 
61 			result.Add("result","success"); 
62 			result.Add("code","0"); 
63 			result.Add("screenlock_QR_no",dt.Rows[0]["screenlock_QR_no"]); 
64 			return result;  
65 		} 
66 		catch(Exception e) 
67 		{ 
68 			BasePage basePage = new BasePage(); 
69 			return basePage.GetErrorMessage(e, param);  
70 
 
71 		}	 
72 } 
73 
 
74 
 
75 
 
76 } 
77 
 
78 
 
79 // DataBase  
80 public DataTable SetScreenlockQR(Dictionary<string ,object> param) 
81 
 
82 { 
83 	//Procedure Name  
84 	string spName = "sp_screenlock_QR_insert"; 
85 	string connectionString = ConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString; 
86 	List<SqlParameter> sqlParam = new List<SqlParameter> () ;  
87 	//Parameter( )  
88 	sqlParam.Add(new SqlParameter("@screenlock_QR_no", m_dbHelper.GetValueFromParam(param,"screenlock_QR_no))); 
89 	sqlParam.Add(new SqlParameter("@user_no", m_dbHelper.GetValueFromParam(param,"user_no"))); 
90 	sqlParam.Add(new SqlParameter("@screenlock_approve_YN",m_dbHelper.GEtValueFromParam(param,"screenlock_approve_YN"))); 
91 	return m_dbHelper.ExecuteSP(spName , connectionString , sqlParam); 
92 
 
93 
 
94 
 
95 
 
96 }	 
97 //QR approve 작동 원리  
98 // *원리 : 초기 default 값은 N 이다 => 즉 , QR 이 스캔 되지않았을땐 N 으로 인식 , 
99 // => 반면 , QR 이 스캔되었을때는 Y 로 인식이 되게 한다  
100 
 
101 public Dictionary<string , object> SetScreenLockApprove(Dictionary<stirng , object> param) 
102 { 
103 	Dictionary<stirng , obejct> result = null; 
104 	try 
105 	{ 
106 		// 초기 screenlock_approve_YN 의 값을 N 으로 해둔다 => 아직 QR 스캔하기 전  
107 		param["screenlock_approve_YN"] = "N"; 
108 		using (DataTable dt = dac.GetScreenlockQR(param)) 
109 	 
110 	} 
111 		//QR 코드가 정상적으로 스캔 되지 않았을 경우 
112 		if(dt == null || dt.Rows.Count <= 0) 
113 		{ 
114 			result = new Dictionary<string ,object>(); 
115 			result.Add("result","failure"); 
116 			result.Add("code","1"); 
117 			result.Add("message","모바일앱에서 QR코드를 스캔해주세요."); 
118 			return result; 
119 
 
120 		} 
121 		param["screenlock_QR_no"] = dt.Rows[0]["screenlock_QR_no"]; 
122 		param["screenlock_approve_YN"] = "Y"; 
123 		using (DataTable dt2  = dac.SetScreenlockQR(param)) 
124 		{ 
125 			//DataTable != null 일경우  
126 			if(dt2 == null || dt2.Rows.Count <= 0) 
127 			{ 
128 				result = new Dictionary<string , object> () ;  
129 				result.Add("result", "failure"); 
130 				result.Add("code","3"); 
131 				return result; 
132 
 
133 
 
134 			} 
135 			result = new Dictionary<string , object>(); 
136 			result.Add("result","success"); 
137 			result.Add("code","0"); 
138 			return result; 
139 
 
140 		} 
141   catch (Exception e) 
142             { 
143                 BasePage basePage = new BasePage(); 
144                 return basePage.GetErrorMessage(e, param); 
145             } 
146 
 
147 
 
148 } 
149 
 
150 //dac.GetScreenlockQR 을 추적해보면  
151 
 
152 public DataTable SetScreenlockQR(Dictionary<string , object> param) 
153 { 
154 	// param ==> @screenlock_approve_YN 
155 	string spName = "sp_screenlock_QR_select"; 
156             string connectionString = ConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString; 
157             List<SqlParameter> sqlParam = new List<SqlParameter>(); 
158 	// 여기에선 parameter 로 전달해준 @screenlock_approve_YN 과 , user_no 를 추가해주어서 어떤 유저의 QR 을 승인할지를 전달해주는 param 
159 
 
160             sqlParam.Add(new SqlParameter("@user_no", m_dbHelper.GetValueFromParam(param, "user_no"))); 
161             sqlParam.Add(new SqlParameter("@screenlock_approve_YN", m_dbHelper.GetValueFromParam(param, "screenlock_approve_YN"))); 
162             return m_dbHelper.ExecuteSP(spName, connectionString, sqlParam); 
163 
 
164 
 
165 
 
166 
 
167 
 
168 } 
169 //procedure  
170 CREATE PROCEDURE [dbo].[sp_screenlock_QR_select]	 
171 	-- @user_no , @screenlock_approve_YN 이 parameter 로 전달되는 상황 	 
172 	@user_no					INT, 
173 	@screenlock_approve_YN		CHAR(1)	 
174 
 
175 AS 
176 BEGIN	 
177 	SET NOCOUNT ON; 
178 
 
179 	SELECT TOP 1 SQ.screenlock_QR_no, SQ.screenlock_approve_YN, SQ.screenlock_approve_time, SQ.createdTime 
180 	FROM [dbo].[tb_screenlock_QR] SQ WITH(NOLOCK) 
181 	WHERE SQ.user_no = @user_no 
182 	AND SQ.screenlock_approve_YN = CASE WHEN @screenlock_approve_YN IS NULL THEN SQ.screenlock_approve_YN ELSE @screenlock_approve_YN END 
183 	ORDER BY createdTime DESC 
184 END 
185 
 
