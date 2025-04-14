

import { Component, OnInit, ViewChildren, QueryList } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonModule, DecimalPipe } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PageChangedEvent, PaginationModule } from 'ngx-bootstrap/pagination';
import { StoryService } from './services/story.service';
import { Story } from './models/story.model';
import { HttpClient, HttpClientModule, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  standalone: true,
  providers: [
    StoryService, HttpClient, DecimalPipe
  ],
  imports:[       
    CommonModule, 
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    PaginationModule,
  ]
})

export class AppComponent implements OnInit {

  constructor(private storyService: StoryService){
    this.storyService.GetStories().subscribe({
      next: (data) => {
        this.stories = data;
        console.log(this.stories);
      },
      error: (error) => console.log(error)
    })
  }

  stories:Story[] = [];
  
  currentPage = 1;
  itemsPerPage = 10; // default 10 per page

  // Get the paginated slice of data
  get paginatedData() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.stories.slice(start, start + this.itemsPerPage);
  }

  // Handle page change event
  pageChanged(event: any) {
    this.currentPage = event.page;
    this.itemsPerPage = event.itemsPerPage ?? this.itemsPerPage; 
    // (some pagination components emit itemsPerPage if it's changeable by the user)
  }

  ngOnInit(): void {
  
  }

}
