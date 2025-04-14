import { Injectable, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { Story } from '../models/story.model';

@Injectable({ providedIn: 'root' })
export class StoryService {

    baseUrl = "https://localhost:7117/api"
    constructor(private http: HttpClient) { }

    // createAction(action: ActionDTO): Observable<ActionTableItem> {
    //     return this.http.post<BaseResponse<ActionTableItem>>(`${this.baseUrl}/actions`, action)
    //         .pipe(
    //             map(response => response.data),
    //             tap(data => {
    //                 if (isDevMode)
    //                     console.log(data)
    //             })
    //         )
    // }

    // getActionById(id: number): Observable<ActionDTO> {
    //     return this.http.get<BaseResponse<ActionDTO>>(`${this.baseUrl}/actions/${id}`)
    //         .pipe(
    //             map(response => response.data),
    //             tap(data => {
    //                 if (isDevMode)
    //                     console.log(data)
    //             })
    //         )
    // }

    GetStories(): Observable<Story[]> {
        
        return this.http.get<Story[]>(`${this.baseUrl}/Stories/NewStories`)
            .pipe(
                tap(data => {
                    if (isDevMode())
                        console.log(data)
                }
            )
            )
    }

}
