������ ���񽺷� �ø��� ���� 


1 ���� ��ġ�� �ý��� ���� �� EndPoint ���� 
	1) Visual Studio���� ����ɼ��� Release, Any CPU�� ����
	2) �ý��� ����
		������ ���� ������(services.msc) �����ϰ� atoihomeservice ����
		Visual Studio ������ ����� ������Ʈ ����
		f:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
		�⼳ġ�� ����(C:\Program Files (x86)\atoihome)�� �̵��ؼ� �������� (installutil -u atoihomeservice.exe)
		�����ǿ��� ���� ���α׷� ����
	

%2-6 ������ service contract�� ������� �ʾ����� �������� �ʾƵ� �˴ϴ�

2 AhoiHome ������ �� ����
	AtoiHome app.config���� rest ep �ּ�ó�� �� ����, ����

3 AtoiHomeManager servicereference ������Ʈ (VS ���)
	TextTransferServiceSoap ������ �����̵�

4 AtoiHome ������ �� App.config���� �ּ�ó���� rest ep �����ϰ� �������� �� ����

5 AtoiHomeManager ����

6 ������ �̹��� ���ε� �ؼ� �������� Ȯ���� AtoiHome ����

7  �۹���
 	1) Solution Rebuild
	2) Setup ������Ʈ���� ���� ������ ����
	3) Setup ������Ʈ���� ��ġ (��ġ���� C:\Program Files (x86)\atoihome)

8 �����켭�񽺷� ���  (�ڵ�ȭ �� �� �ִ� ����� ã�ƾߵ�)
	Visual Studio ������ ����� ������Ʈ ����
		f:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
	��ġ�� ������ �̵��ؼ� (C:\Program Files (x86)\atoihome)
	installutil atoihomeservice.exe
	������ ���� ������(services.msc) �����ϰ� atoihomeservice ����
	

9 ������ ���� ����
	1) PowerShell �����ϰ� ��ġ����/Log ������ �̵��ؼ� "Get-Content log.txt -wait" ����
	2) AtoiHomeManager ����
	3) ���̳� �Ŀ��� �̹��� ���ε� �ϰ� �������� Ȯ��
