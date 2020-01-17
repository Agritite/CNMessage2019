import socket 
import select 
import sys 
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
# online user
connected_client = {}
#[from, to, msg]
online_msg = []
#[from, to, msg]
offline_msg = []
#[from, to, filename, data]
file_msg = []
#historical 



def clientthread(conn, addr, online_msg, offline_msg, file_msg): 
  
    # sends a message to the client whose user object is conn 
    # conn.send("Welcome to this chatroom!") 
    login_flag = False 
    current_username = ''

    while True: 
        try: 
            # reading: non-blocking, timeout = 1s
            readable, _, _ = select.select([conn], [], [], 1)
            if conn in readable :
                status = int(conn.recvall(1).decode())
            else : # nothing to read
                status = -1 

            # login
            if status == 0 :
                username_size = int.from_bytes(conn.recvall(4), byteorder='little')
                username = conn.recvall(username_size).decode()
                password_size = int(conn.recvall(4).decode())
                password = conn.recvall(password_size).decode()

                #login success
                if username in user_list and user_list[username] == hash(password) :
                    conn.send(bytes([1]))
                    login_flag = True
                    current_username = username 
                    connected_client[current_username] = conn
                    print('login succuss: ' + current_username)
                else :
                    conn.send(bytes([0]))
                    print('login failed: ' + username)
                    conn.close()
                    break

            # register 
            elif status == 1 :
                username_size = int(conn.recvall(4).decode())
                username = conn.recvall(username_size).decode()
                password_size = int(conn.recvall(4).decode())
                password = conn.recvall(password_size).decode()
                # register failed
                if username in user_list :
                    conn.send(bytes([0]))
                    print('username used.')
                else :
                    conn.send(bytes([1]))
                    login_flag = True
                    current_username = username
                    print('registered: ' + username)
                    conn.close()
                    break
            
            # query history
            # elif status == 2 :
                # TODO

            # send msg
            elif status == 3 :
                username_size = conn.recvall(4)
                username_size = int.from_bytes(username_size, byteorder='little')
                dest = conn.recvall(username_size).decode()

                msg_size = conn.recvall(4)
                msg_size = int.from_bytes(msg_size, byteorder='little')
                message = conn.recvall(msg_size).decode()

                # prints the message of the user who just sent the message on the server terminal
                print(current_username + ' to ' + dest + ' : ' + message)
                # put msg in list
                if dest not in user_list :
                    conn.send(bytes([0]))
                elif dest in connected_client : # online
                    OnlineMsg.append([current_username, dest, message])
                else : #offline
                    OfflineMsg.append([current_username, dest, message])

            # file transfer
            elif status == 4 :
                username_size = conn.recvall(4)
                username_size = int.from_bytes(username_size, byteorder='little')
                dest = conn.recvall(username_size).decode()

                fname_size = conn.recvall(4)
                fname_size = int.from_bytes(msg_size, byteorder='little')
                filename = conn.recvall(fname_size)

                file_size = conn.recvall(4)
                file_size = int.from_bytes(file_size, byteorder='little')
                filedata = conn.recvall(file_size)

                # prints the message of the user who just sent the file on the server terminal
                print(current_username + 'send file to ' + dest)
                # put msg in list
                if dest not in connected_client : #offline
                    conn.send(bytes([0]))
                else : #online
                    file_msg.append([current_username, dest, filename, filedata])

            #log out
            elif status == 5 :
                conn.close()
                del connected_client[current_username]
                break


            # TODO : disconnect ?


            # check online msg
            for msg in online_msg :
                if msg[1] == current_username :
                    conn.send(bytes([13]))

                    conn.send(bytes(len(msg[0])))
                    conn.send(msg[0].encode())

                    conn.send(bytes(len(msg[2])))
                    conn.send(msg[2].encode())
                    
                    OnlineMsg.remove(msg)

            # check online file
            for msg in file_msg :
                if msg[1] == current_username :
                        conn.send(bytes([14]))

                        conn.send(bytes(len(msg[0])))
                        conn.send(msg[0].encode())

                        conn.send(bytes(len(msg[2])))
                        conn.send(msg[2].encode())

                        file_msg.remove(msg)

        except: 
            print('exception occured.')
            continue
        
  
while True: 

    conn, addr = server.accept() 

    # prints the address of the user that just connected 
    print(addr[0] + " connected")
  
    # creates and individual thread for every user
    start_new_thread(clientthread, (conn, addr, online_msg, offline_msg))     
  
conn.close() 
server.close() 