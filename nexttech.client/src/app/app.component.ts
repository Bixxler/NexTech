

import { Component } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PageChangedEvent, PaginationModule } from 'ngx-bootstrap/pagination';
import { StoryService } from './services/story.service';
import { Story } from './models/story.model';
import { HttpClient } from '@angular/common/http';

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

export class AppComponent {
  stories:Story[] = [];
  currentPage = 1;
  itemsPerPage = 10; 
  searchTerm: string = '';
  filteredStories: any[] = [];
  loading = true;

  constructor(private storyService: StoryService, private httpClient: HttpClient) {
    this.fetchStories();
  }

  fetchStories(){
    this.loading = true;
    this.storyService.GetStories().subscribe({
      next: (data) => {
        this.stories = data;
        this.filteredStories = data; // Initialize filteredStories with all stories
        this.loading = false;
      },
      error: (error) => 
      {
        console.error('Error fetching stories:', error);
        this.loading = false;
      }
     })
  }

  get paginatedData() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    return this.filteredStories.slice(start, end);
  }

  pageChanged(event: any) {
    this.currentPage = event.page;
  }

  pageSizeChanged(event: any) {
    this.itemsPerPage = +event.target.value;
    this.currentPage = 1;
  }
  
  onSearchTermChanged(event: any) {
    this.searchTerm = event.target.value.toLowerCase();

    if(this.searchTerm !== '') {
      this.filteredStories = this.stories.filter(story => 
        story.title.toLowerCase().includes(this.searchTerm) ||
        story.url.toLowerCase().includes(this.searchTerm)
      );
    
      this.currentPage = 1; // reset back to first page
    }
    else{
      this.filteredStories = this.stories;
    }
  }
}
