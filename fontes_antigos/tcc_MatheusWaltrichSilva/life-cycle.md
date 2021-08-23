#### Aquário Virtual

- Necessidades para manter ciclo de vida
   - Alimentação
      - A cada X minutos, os peixes devem receber comida para que possam continuar sobrevivendo
         - Caso recebam pouca comida, alimentará somente alguns peixes (necessário verificar como será feito este controle)
         - Caso recebam muita comida, poderá poluir o aquário
   - Temperatura
      - O cálculo da temperatura deverá ser uma equação entre a temperatura do aquecedor e a fonte de luz externa
      - De acordo com a luz externa obtida, será aumentada a temperatura do aquário. Se a luz externa for constante, aumentará a temperatura do aquário aos poucos.
      - Caso não possua luz externa, deverá diminuir a temperatura do aquário.
      - Em alguns casos, de tempo em tempo a temperatura da água começará a diminuir, simulando uma mudança climática
         - Nestas situações, haverá a necessidade de ativar o aquecedor para regular a temperatura
   - Luminosidade
      - Conforme a luz externa é captada, o aquário se mantém iluminado
      - Sem lux externa, será diminuída a quantidade de luz dentro do aquário (directional light)
      - A falta ou excesso de luz pode impactar no ecossistema
         - Caso falte luz, as plantas devem começar a morrer (diminuir)
         - Caso tenha luz por muito tempo, as plantas deverão crescer (estabelecer limite?)
   - Poluição
      - Pode ter um sistema básico de poluição, um botão em tela que efetua a limpeza do aquário, mas não é possível usar a todo momento
         - Começa com 2 unidades para limpar o aquário, com o tempo pode ganhar mais