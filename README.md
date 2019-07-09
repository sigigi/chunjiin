# c# .net chunjiin 소스코드

아래 원작자님 [kys-zeda/chunjiin](https://github.com/kys-zeda/chunjiin) 님의 android java 천지인 소스코드를 기반으로
.net c# 언어 컨버팅 및 몇가지 소소한 기능 및 보완코드를 넣은 버전입니다.
원작자님의 뜻대로 저또한 천지인 코드를 찾다가 올려주신 내용을 보고 제가 필요한 부분이 있어 수정 후 사용하였고, 
관련 개발자님들이 필요한 경우가 있을 것 같아 올려봅니다.
라이센스 역시 MIT 라이센스를 적용합니다.

 * dotnetChunjiinKeyPad/ChunjiinKeyPad/Form1.cs 파일이 예제 파일입니다.
 * Visual Studio 2017 Community 버전으로 작성되었습니다.
 
 
 * 변경내용
 > 1. ㅏ -> ㅑ  상태에서 다시 2번 점이 들어오면 ㅏ 로 표현
 > 2. ㅜ -> ㅠ 1.번과 동일  
 > 3. Key 버튼 클릭시 마다 타이머를 이용하여 현재 입력을 종료하고 다음 입력모드로 전환 (현재 1초로 설정)
 > 4. TextBox에 드래그 하여 텍스트를 선택하고 키 입력을 할 경우에 선택된 부분을 삭제하고 새로운 키를 입력하는 부분 추가.
 > 5. delete() 원본 대비 일부 수정


원작자님 Readme
----------------------------------------------------------


Android Java 천지인 소스코드 입니다.

Java, Android 환경에서 작동합니다.

예제는 Eclipse에서 작성되었습니다.

EditText 1개, Button 13개를 등록해서 사용합니다.

-저작권 표시-

MIT 라이센스(http://opensource.org/licenses/mit-license.php)를 적용합니다. 

오픈 소스로 공개하는 만큼 저작권은 꼭 지켜 줬으면 합니다.


# chunjiin

Chunjiin is One way of Keyboard type for input Korean

Automata Class for JAVA, Android.



example is eclipse project.

Layout include 1 EditText and 13 Buttons.

Class must needs to regist '1 EditText' and '13 Buttons'.

License : MIT

contact : irineu2@naver.com


---------------------------



