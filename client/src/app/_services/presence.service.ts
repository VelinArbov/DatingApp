import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import * as signalR from '@microsoft/signalr'
import { ToastrService } from 'ngx-toastr';
import { User } from '../_modules/user';
import { runInThisContext } from 'vm';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: signalR.HubConnection;
  private onlineUsersSource= new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toast:ToastrService) { }

  createHubConnection(user: User){
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build()

    this.hubConnection.start()
    .catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username=> {
      this.toast.info(username + ' has connected');
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.toast.warning(username + ' is disconnected');
    })

    this.hubConnection.on('GetOnlineUsers',(usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
      
    })
  }


  stopHubConnection(){
    this.hubConnection.stop()
    .catch(error => console.log(error))
  }

}
