import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { packagerGuard } from './packager-guard';

describe('packagerGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => packagerGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
