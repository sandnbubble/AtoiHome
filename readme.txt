윈도우 서비스로 올리는 절차 


1 기존 설치된 시스템 제거 및 EndPoint 수정 
	1) Visual Studio에서 빌드옵션을 Release, Any CPU로 설정
	2) 시스템 제거
		윈도우 서비스 관리자(services.msc) 실행하고 atoihomeservice 중지
		Visual Studio 개발자 명령행 프람프트 실행
		f:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
		기설치된 폴더(C:\Program Files (x86)\atoihome)로 이동해서 서비스제거 (installutil -u atoihomeservice.exe)
		제어판에서 기존 프로그램 제거
	

%2-6 절차는 service contract가 변경되지 않았으면 수행하지 않아도 됩니다

2 AhoiHome 컴파일 후 실행
	AtoiHome app.config에서 rest ep 주석처리 후 빌드, 실행

3 AtoiHomeManager servicereference 업데이트 (VS 기능)
	TextTransferServiceSoap 선택후 업데이드

4 AtoiHome 종료한 후 App.config에서 주석처리한 rest ep 복구하고 컴파일한 후 실행

5 AtoiHomeManager 실행

6 폰으로 이미지 업로드 해서 정상인지 확인후 AtoiHome 종료

7  앱배포
 	1) Solution Rebuild
	2) Setup 프로젝트에서 이전 배포판 제거
	3) Setup 프로젝트에서 설치 (설치폴더 C:\Program Files (x86)\atoihome)

8 윈도우서비스로 등록  (자동화 할 수 있는 방법을 찾아야됨)
	Visual Studio 개발자 명령행 프람프트 실행
		f:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
	설치된 폴더로 이동해서 (C:\Program Files (x86)\atoihome)
	installutil atoihomeservice.exe
	윈도우 서비스 관리자(services.msc) 실행하고 atoihomeservice 시작
	

9 윈도우 서비스 시험
	1) PowerShell 실행하고 설치폴더/Log 폴더로 이동해서 "Get-Content log.txt -wait" 실행
	2) AtoiHomeManager 실행
	3) 폰이나 컴에서 이미지 업로드 하고 정상인지 확인
