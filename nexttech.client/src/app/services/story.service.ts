import { Injectable, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Story } from '../models/story.model';

@Injectable({ providedIn: 'root' })
export class StoryService {

    constructor(private http: HttpClient) { }

    GetStories(): Observable<Story[]> {
        let string = isDevMode() ? 'http://localhost:7236/api/stories' : '/api/stories'
        return this.http.get<Story[]>(string)
            .pipe(
                tap(data => {
                    if (isDevMode())
                        console.log(data)
                }
            )
        )
    }
}
