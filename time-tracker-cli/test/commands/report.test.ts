import {expect, test} from '@oclif/test'

describe('report', () => {
  test
  .stdout()
  .command(['report'])
  .it('runs report', ctx => {
    expect(ctx.stdout).to.contain('report world')
  })

  test
  .stdout()
  .command(['report', '--name', 'jeff'])
  .it('runs report --name jeff', ctx => {
    expect(ctx.stdout).to.contain('report jeff')
  })
})