import socket 
import select 
import sys 
from Queue import Queue
from _thread import *
  
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1) 
  
# checks whether sufficient arguments have been provided 
if len(sys.argv) != 3: 
    print("Correct usage: script, IP address, port number")
    exit() 
  
# takes the first argument from command prompt as IP address 
IP_address = str(sys.argv[1]) 
Port = int(sys.argv[2]) 
  
# The client must be aware of these parameters 
server.bind((IP_address, Port)) 
# listens for 10 active connections. 
server.listen(10) 

# userlist[username]=hash(pw)
user_list = {}
# connected_client[username] = socket
connected_client = {}
#list of sockets
list_of_clients = [] 
#[dest, msg]
online_msg = Queue()
#[dest, msg]
offline_msg = Queue()


def clientthread(conn, addr, qOnline, qOffline): 
  
    # sends a message to the client whose user object is conn 
    # conn.send("Welcome to this chatroom!") 
    login_flag = False 
    current_username = ''
    msg_log = []

    while True: 
        try: 
            # reading: non-blocking, timeout = 1s
            readable, _, _ = select.select([conn], [], [], 1)
            if conn in readable :
                status = int(conn.recv(1).decode())
            else : # nothing to read
                status = -1 

            # login
            if status = 0 :
                username_size = int(conn.recv(4).decode())
                username = conn.recv(username_size).decode()
                password_size = int(conn.recv(4).decode())
                password = conn.recv(password_size).decode()

                #login success
                if username in user_list and user_list[username] == hash(password) :
                    conn.send('1')
                    login_flag = True
                    current_username = username 
                    connected_client[current_username] = conn
                    print('login succuss: ' + current_username)
                else :
                    conn.send('0')
                    print('login failed: ' + username)

                # check offline message
                if login_flag = True :
                    # TODO: check offline message

            # register 
            else if status = 1 :
                username_size = int(conn.recv(4).decode())
                username = conn.recv(username_size).decode()
                password_size = int(conn.recv(4).decode())
                password = conn.recv(password_size).decode()
                # register failed
                if username in user_list :
                    conn.send('0')
                    print('username used.')
                else :
                    conn.send('1')
                    login_flag = True
                    current_username = username
                    print('registered: ' + username)

            # query history
            else if status = 2 :
                # TODO

            # send msg
            else if status = 3 :
                username_size = int(conn.recv(4).decode())
                dest = conn.recv(username_size).decode()
                msg_size = int(conn.recv(4).decode())
                message = conn.recv(msg_size).decode()

                if message : 
                    # prints the message and address of the user who just sent the message on the server terminal
                    print(current_username + ' to ' + dest " : " + message)
                    # send message
                    dest_sock = connected_client[dest]
                    send_msg(message, dest_sock) 
                # connection broken
                else: 
                    """message may have no content if the connection 
                    is broken, in this case we remove the connection"""
                    remove(conn)

            # file transfer
            else if status = 4 :
                # TODO

            # check 


            

        except: 
            continue
  
# send msg to destination client 
def send_msg(message, client): 
    if client in list_of_clients: 
        try: 
            #TODO : client format
            client.send(message.encode())
        except: 
            # if the link is broken, we remove the client 
            client.close()
            remove(client) 
            offline_msg

# remove socket from list_of_clients
def remove(connection): 
    if connection in list_of_clients: 
        list_of_clients.remove(connection) 
        #TODO: remove from connected_client
        for sock in 
  
while True: 
  
    conn, addr = server.accept() 
  
    # Maintains a list of clients for ease of sending a message to available people in the chatroom
    list_of_clients.append(conn) 
  
    # prints the address of the user that just connected 
    print(addr[0] + " connected")
  
    # creates and individual thread for every user
    start_new_thread(clientthread,(conn,addr,online_msg,offline_msg))     
  
conn.close() 
server.close() 