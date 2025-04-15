import { Injectable, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { Story } from '../models/story.model';

@Injectable({ providedIn: 'root' })
export class StoryService {

    constructor(private http: HttpClient) { }

    GetStories(): Observable<Story[]> {
        return this.http.get<Story[]>(`/stories`)
            .pipe(
                tap(data => {
                    if (isDevMode())
                        console.log(data)
                }
            )
        )
    }
}
