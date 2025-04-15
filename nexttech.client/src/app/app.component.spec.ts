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
    mockStoryService.GetStories.and.returnValue(throwError(() => new Error('API Error')));

    component.fetchStories();
    tick();
    expect(component.loading).toBeFalse();
    expect(component.stories.length).toBe(0);
  }));

  // Test case to check if stories are set correctly
  it('should filter stories by search term', () => {
    component.stories = mockStories;
    component.onSearchTermChanged({ target: { value: 'first' } });

    expect(component.filteredStories.length).toBe(1);
    expect(component.filteredStories[0].title).toBe('First Mock Story');
  });

  // Test case to check if the current page is set correctly
  it('should update current page on pageChanged', () => {
    component.pageChanged({ page: 3 });
    expect(component.currentPage).toBe(3);
  });

  // Test case to check if the page size is updated correctly
  it('should update page size and reset to page 1 on pageSizeChanged', () => {
    component.pageSizeChanged({ target: { value: '25' } });
    expect(component.itemsPerPage).toBe(25);
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