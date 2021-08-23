# Virtual Aquarium
Virtual Aquarium it's my undergraduate thesis to simulate an aquarium that is controlled by Tangible User Interface

The project it's a Unity Game that is controlled by NodeMCU (ESP8266). The components connected in ESP8266 send commands to Unity to execute actions in the game.

Languages:
   - C#
   - C


Na tela inicial do aquário temos o botão de configuração, onde você pode ativar a integração com o IOT e o multiplayer.  
Para a utilização do multiplayer é necessário ter o campo Servidor preenchido corretamente com o ip do dispositivo que está executando o aquário como host.  
Ainda na configuração pode-se ativar a câmera para modo desenvolvimento, onde inverte a posição da câmera no aquário, mas esta opção deve estar marcada tanto no dispositivo que está executando o host, como nos clientes que vão se conectar.  
A configuração do host no multiplayer é somente necessário para os clientes, caso selecione a opção multiplayer e clique em jogar aquário irá iniciar o host e nesta tela o ip para ser informado nos clientes.  
Para iniciar os clientes basta nas configurações marcar a opção multiplayer e informar o ip do dispositivo host e na tela inicial selecionar a opção jogar RV.  
