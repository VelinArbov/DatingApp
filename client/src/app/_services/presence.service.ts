import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import * as signalR from '@microsoft/signalr'
import { ToastrService } from 'ngx-toastr';
import { User } from '../_modules/user';
import { runInThisContext } from 'vm';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: signalR.HubConnection;
  private onlineUsersSource= new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toast:ToastrService, private router: Router) { }

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
      this.onlineUsers$.pipe(take(1)).subscribe(usernames=> {
        this.onlineUsersSource.next([...usernames, username])
      });
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames =>{
        this.onlineUsersSource.next([...usernames.filter(x=> x !== username)])
      })
    })

    this.hubConnection.on('GetOnlineUsers',(usernames: string[]) => {
      this.onlineUsersSource.next(usernames);

    this.hubConnection.on('NewMessageReceived', ({username, knowAs})=> {
      this.toast.info(username + ' has sent you a new message!')
      .onTap
      .pipe(take(1))
      .subscribe(()=> this.router.navigateByUrl('/members/' + username + '?tab=3'));
    })
      
    })
  }


  stopHubConnection(){
    this.hubConnection.stop()
    .catch(error => console.log(error))
  }

}
