'use strict'

const dgram = require('dgram');
const process = require('process');
const net = require('net');

const client = new net.Socket();

const PORT = 5000;
const IP_ADDRESS = '227.55.77.99';

const socket = dgram.createSocket({type: 'udp4', reuseAddr: true});

socket.bind(PORT);

let remoteIP;

const sendMessage = () => {  
  const message = Buffer.from(`AQUARIUM_01`);
  socket.send(message, 0, message.length, PORT, IP_ADDRESS, () => console.log(`Sending message: ${message}`));
}

socket.on('listening', () => {
  socket.addMembership(IP_ADDRESS);
  sendMessage()
  const address = socket.address();
  console.log(`UDP socket listening on ${address.address}:${address.port} pid: ${process.pid}`);
});

socket.on('message', (message, rinfo) => {
  if (String(message) === 'AQUARIUM_01' && rinfo.address != "192.168.15.13") {    
    remoteIP = rinfo.address;
    client.connect(8080, remoteIP, function() {
      console.log('Connected');
      client.write('TESTE');
    });

    client.on('data', function(data) {
      console.log('Received: ' + data);
    });
    
    client.on('close', function() {
      console.log('Connection closed');
    });
  }
  console.info(`Message from: ${rinfo.address}:${rinfo.port} - ${message}`);
});