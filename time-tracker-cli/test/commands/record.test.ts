import {expect, test} from '@oclif/test'
import { throws } from 'assert';

describe('record', () => {
  // test for validating missing argument of project that is required

  test
    .stdout()
    .command(['record', 'ford'])
    .it('runs record ford', ctx => {
      expect(ctx.stdout).to.contain(
        `logged 8 hours today for ford`
      )
    })

  test
    .stdout()
    .command(['record', 'ford', '7', '-d 2018-08-10'])
    .it('runs record ford 7 -d 2018-08-10', ctx => {
      expect(ctx.stdout).to.contain(
        `logged 7 hours on 2018-08-10 for ford`
      )
    })

})