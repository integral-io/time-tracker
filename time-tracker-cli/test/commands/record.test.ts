import {expect, test} from '@oclif/test'
import { throws } from 'assert';

describe('record', () => {
  // test for validating missing argument of project that is required

  test
  .stdout()
  .command(['record', 'ford'])
  .it('runs record ford', ctx => {
    expect(ctx.stdout).to.contain('Logged 8 on today for ford')
  })
})