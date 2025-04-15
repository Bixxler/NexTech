

import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';
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

export class AppComponent implements OnInit {
  stories:Story[] = [];
  currentPage = 1;
  itemsPerPage = 10; 
  searchTerm: string = '';
  filteredStories: any[] = [];
  loading = true;
  errorMesssage: string = '';

  constructor(private storyService: StoryService, private httpClient: HttpClient) {
  }

  ngOnInit(): void {
    // Initialize the component and fetch stories
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
        this.stories = []; // Reset stories to an empty array on error
        this.filteredStories = []; // Reset filteredStories to an empty array on error
        this.loading = false;
        error.message = error.message;
        console.error('Error fetching stories:');
      }
     })
  }

  get paginatedData() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    return this.filteredStories.slice(start, end);
  }

  pageChanged(event: any) {
    console.log('Page changed:', event.page);
    // Update the current page based on the event emitted by the pagination component
    this.currentPage = event.page;
    console.log('Current page:', this.currentPage);
  }

  pageSizeChanged(event: any) {
    this.itemsPerPage = event.target.value;
    this.currentPage = 1;
  }
  
  onSearchTermChanged(event: any) {
    // Get the search term from the input field and convert it to lowercase
    this.searchTerm = event.target.value.toLowerCase();

    // Filter stories based on the search term
    // If search term is empty, show all stories
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
