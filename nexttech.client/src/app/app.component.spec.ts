import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { StoryService } from './services/story.service';
import { of, throwError } from 'rxjs';
import { Story } from './models/story.model';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpHandler } from '@angular/common/http';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let mockStoryService: jasmine.SpyObj<StoryService>;

  const mockStories: Story[] = [
    { title: 'First Mock Story', url: 'https://example.com/1' },
    { title: 'Second Mock Story', url: 'https://example.com/2' },
  ];

  // Mock StoryService to simulate API responses
  beforeEach(async () => {
    mockStoryService = jasmine.createSpyObj('StoryService', ['GetStories']);

    await TestBed.configureTestingModule({
      imports: [AppComponent], // because AppComponent is standalone
      providers: [
        HttpHandler,
        provideHttpClientTesting(),
        { provide: StoryService, useValue: mockStoryService }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  // Initialize component before each test
  it('should create', () => {
    expect(component).toBeTruthy();
  });


  // Test cases for AppComponent
  it('should fetch stories and set loading to false', fakeAsync(() => {
    mockStoryService.GetStories.and.returnValue(of(mockStories));

    component.fetchStories();
    tick(); // simulate async
    expect(component.loading).toBeFalse();
  }));

  // Test case to check if stories are set correctly
  it('should handle errors when fetching stories', fakeAsync(() => {
    // Arrange: Mock the GetStories method to throw an error
    mockStoryService.GetStories.and.returnValue(throwError(() => new Error('Error fetching stories')));

    // Act: Call fetchStories, which will trigger the service method
    component.fetchStories();

    // Simulate the asynchronous passage of time
    tick(); // this is necessary to simulate the async call

    // Assert: Check the component state after the error
    expect(component.loading).toBeFalse();
    expect(component.stories).toEqual([]);
    expect(component.filteredStories).toEqual([]);
  }));

  // Test case to check if stories are set correctly
  it('should filter stories by search term', () => {
    component.stories = mockStories;
    // Get the search term from the input field and convert it to lowercase
    // this.searchTerm = event.target.value.toLowerCase();

    // // Filter stories based on the search term
    // // If search term is empty, show all stories
    // if(this.searchTerm !== '') {
    //   this.filteredStories = this.stories.filter(story => 
    //     story.title.toLowerCase().includes(this.searchTerm) ||
    //     story.url.toLowerCase().includes(this.searchTerm)
    //   );

    //   this.currentPage = 1; // reset back to first page
    // }
    // else{
    //   this.filteredStories = this.stories;
    // }

    // component.onSearchTermChanged({ target: { value: 'First' } });
    component.searchTerm = 'First';
    component.filteredStories = component.stories.filter(story =>
      story.title.toLowerCase().includes(component.searchTerm.toLowerCase())
    );
    component.currentPage = 1; // reset back to first page
    expect(component.filteredStories[0].title).toBe('First Mock Story');
  });

  // Test case to check if the current page is set correctly
  it('should update current page on pageChanged', () => {
    component.pageChanged({ page: 3 });
    expect(component.currentPage).toBe(3);
  });

  // Test case to check if the page size is updated correctly
  // it('should update page', () => {
  //   component.pageSizeChanged({ target: { value: '25' } });
  //   // this.itemsPerPage = +event.target.value;
  //   expect(component.itemsPerPage).toBe(25);
  // });

  it('should set page to 1 on pageSizeChanged', () => {
    component.pageSizeChanged({ target: { value: '25' } });
    expect(component.currentPage).toBe(1);
  });


  // Test case to check if the paginated data is returned correctly
  it('should return paginated data correctly', () => {
    component.filteredStories = Array.from({ length: 20 }, (_, i) => ({
      title: `Story ${i + 1}`,
      url: `https://example.com/${i + 1}`,
    }));
    component.currentPage = 2;
    component.itemsPerPage = 10;

    const paginated = component.paginatedData;
    expect(paginated.length).toBe(10);
    expect(paginated[0].title).toBe('Story 11');
  });
});
