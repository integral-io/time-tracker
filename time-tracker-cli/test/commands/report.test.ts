import {expect, test} from '@oclif/test'
import * as fs from 'fs';

describe('report', () => {

  let myData;

  fs.openSync('../mock-data/username-hours.csv', '')

  test
    .stdout()
    .command(['report'])
    .it('runs report', ctx => {
      expect(ctx.stdout).to.contain('report world')
    })
})